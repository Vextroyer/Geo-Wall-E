/*
This class acts as an intermediary between the user interface and the different compiler components.
It receives G# source code and produces drawable objects. This objects must be handled
by the specific drawing interface. If the compilation fails it return the errors occurred
at compile time.
*/

namespace GSharpCompiler;

public static class Compiler
{
    //Compile from a file containing the source code.
    public static Response CompileFromFile(string path, Flags? flags = null){
        string sourceCode = "";
        List<Error> errors = new List<Error>();
        List<IDrawable> drawables = new List<IDrawable>();
        try{
            sourceCode = Utils.GetSourceFromFile(path);
        }catch(Exception e){
            errors.Add(new Error(-1,-1,path,e.Message));
            return new Response(errors,drawables,true);
        }
        return CompileFromSource(sourceCode,path,flags);
    }
    //Compile from a string containing the source code.
    public static Response CompileFromSource(string source,string sourceFileName,Flags? flags = null)
    {
        //Use default flags
        if(flags == null)flags = new Flags();

        List<Error> errors = new List<Error>();
        List<IDrawable> elements = new List<IDrawable>();

        try
        {   
            //Scan the source code and produce the tokens.
            // List<Token> tokens = new Scanner(source,sourceFileName,flags.MaxErrorCount,errors).Scan();
            List<Token> tokens = new DependencyResolver(source,sourceFileName,errors,flags.MaxErrorCount).ScanAndResolveDependencies(sourceFileName);

            //Print scanner output
            if(flags.PrintDebugInfo)Utils.PrintTokens(tokens,flags.OutputStream);

            //If errors where found stop the compilation process.
            if(errors.Count > 0)throw new ScannerException();

            //Parse the tokens into an abstract syntax tree and store the tree.
            Program program = new Parser(tokens,flags.MaxErrorCount,errors).Parse();

            //Print parser output
            if(flags.PrintDebugInfo)Utils.PrintAst(program,flags.OutputStream);

            //If errors where found stop the compilation process.
            if(errors.Count > 0)throw new ParserException();

            //Check the semantic of the program.
            new TypeChecker(flags.MaxErrorCount,errors).Check(program);

            //If errors where found stop the compilation process.
            if(errors.Count > 0)throw new TypeCheckerException();

            //Interpret the program to produce drawable objects.
            elements = new Interpreter(flags.OutputStream).Interpret(program);
        }
        catch(CompileTimeException){
            //Scanner,Parser,TypeChecker
        }
        catch(RuntimeException e){
            errors.Add(new Error(e.Line,e.Offset,e.File,e.Message));
        }
        catch (Exception e)
        {
            //Write exception to log.
            errors.Add(new Error(0,0,"","Unexpected error, check log and report to developers"));
            Utils.AppendToLog(e);
        }

        return new Response(errors,elements);
    }
    ///<summary>Takes a source code and returns a new source code with all dependencies resolved.</summary>
    private class DependencyResolver{
        ///<summary>Stores (fileName,sourceCode).</summary>
        Queue<(string,string)> filesToProcess = new Queue<(string,string)>();
        ///<summary>Associate to each fileName its dependencies and code.</summary>
        Dictionary<string,FileInfo> fileInfoTable = new Dictionary<string, FileInfo>();
        ///Compiler configuration parameters.
        private ICollection<Error> Errors {get; set;}
        private int MaxErrorCount {get; set;}
        public DependencyResolver(string source, string fileName,ICollection<Error> errors,int maxErrorCount){
            Errors = errors;
            MaxErrorCount = maxErrorCount;
            filesToProcess.Enqueue((fileName,source));
        }
        public List<Token> ScanAndResolveDependencies(string originalFile){
            Dictionary<string,State> state = new Dictionary<string, State>();//Mantain a state for each file.

            //While files to process exist.
            while(filesToProcess.Count > 0){
                string fileName,sourceCode;
                (fileName,sourceCode) = filesToProcess.Dequeue();//Retrievethe first element from the queue.
                
                if(fileInfoTable.ContainsKey(fileName))continue;//If this file has been already processed ignore it.

                List<Token> tokens = (new Scanner(sourceCode,fileName,MaxErrorCount,Errors)).Scan();//Scan the file.
                //Offer support for printing the scanner output.
                if(Errors.Count > 0)throw new ScannerException();//If Errors were found while scanning throw.
                
                List<string> imports = ParseImports(tokens);//Get the files this file imports.
                fileInfoTable.Add(fileName,new FileInfo(imports,tokens));//Put the info on the table.

                foreach(string fileToImport in imports){
                    //Get the source code from the file.
                    string fileToImportSourceCode = "";
                    try{
                        fileToImportSourceCode = Utils.GetSourceFromFile(fileToImport);
                    }catch(Exception e){
                        Errors.Add(new Error(-1,-1,fileToImport,e.Message));
                        throw new ScannerException();
                    }

                    //Put the imported file on the queue.
                    filesToProcess.Enqueue((fileToImport,fileToImportSourceCode));

                    //Put it on the graph.
                    fileInfoTable[fileName].DependsOn.Add(fileToImport);
                }

                state[fileName] = State.UNPROCESSED;
            }

            List<string> preOrder = new List<string>();//The order on which the files must be assembled to one.
            Stack<string> callStack = new Stack<string>();//Used for detecting cycles.
            ///<summary>Performs a dfs traversal on the dependencie graph.</summary>
            void Dfs(string file){
                state[file] = State.PROCESSING;
                callStack.Push(file);
                foreach(string dependsOn in fileInfoTable[file].DependsOn){
                    switch(state[dependsOn]){
                        case State.UNPROCESSED:
                            Dfs(dependsOn);
                            break;
                        case State.PROCESSED:
                            //Ignore processed nodes.
                            break;
                        case State.PROCESSING:
                            //A cycle, circular dependencies.
                            string message = $"{dependsOn} <- ";
                            while(callStack.Peek() != dependsOn){
                                message += $"{callStack.Pop()} <- ";
                            }
                            message += dependsOn;
                            Errors.Add(new Error(-1,-1,originalFile,"The code has circular dependencies : " + message));
                            throw new CircularDependenciesException();
                    }
                }
                callStack.Pop();
                preOrder.Add(file);
                state[file] = State.PROCESSED;
            }

            Dfs(originalFile);
            List<Token> finalProgram = new List<Token>();
            //Concatenate the different programs.
            foreach(string file in preOrder){
                foreach(Token token in fileInfoTable[file].Code){
                    if(token.Type != TokenType.EOF)finalProgram.Add(token);
                }
            }
            finalProgram.Add(fileInfoTable[originalFile].Code.Last());

            return finalProgram;
        }

        enum State{
            PROCESSED,
            PROCESSING,
            UNPROCESSED
        }
        ///<summary>Takes a list of tokens and produce a list of import files.</summary>
        List<string> ParseImports(List<Token> tokens){
            List<string> imports = (new Parser(tokens,MaxErrorCount,Errors)).ParseImports();//Split the imports from the code
            if(Errors.Count > 0)throw new ParserException();
            return imports;
        }

        ///<summary>Holds information about a file.</summary>
        class FileInfo{
            ///<summary>Files imported by this file.</summary>
            public List<string> Imports {get; private set;}
            ///<summary>Code of this file without import statements.</summary>
            public List<Token> Code {get; private set;}
            ///<summary>The files this file depends on.</summary>
            public List<string> DependsOn {get; set;}
            public FileInfo(List<string> imports,List<Token> code){
                Imports = imports;
                Code = code;
                DependsOn = new List<string>();
            }
        }
    }
    //Data structure to hold the objects produced by the compilation process and the errors.
    public class Response
    {
        public bool HadError { get => errors.Count > 0; }
        public bool HadErrorReadingFile { get; private set; } // If this is set to true the response will contain the message of the error.
        public List<Error> Errors { get => errors; }
        public List<IDrawable> Elements { get => elements; }
        List<Error> errors = new List<Error>();
        List<IDrawable> elements = new List<IDrawable>();//The elements produced by the code.
        public Response(List<Error> _errors, List<IDrawable> _elements,bool hadErrorReadingFile = false)
        {
            errors = _errors;
            elements = _elements;
            HadErrorReadingFile = hadErrorReadingFile;
        }
    }

    //Represents several options that alter the behaviour of the compiler.
    public class Flags{
        //This flags determine wheter the compiler should print the tokens produced by the scanner and the ast produced by the parser.
        public bool PrintDebugInfo {get; set;}
        //This flag is for setting the stream to which the client application wants to redirect the output of the `print` statements and the debug info.
        public TextWriter OutputStream {get; set;}
        /**
        <summary>Stablish a limit of errors to be catched before aborting the compilation process.</summary>
        **/
        private int maxErrorCount;
        /**
        <value>Stablish a limit of errors to be catched before aborting the compilation process. Default value is 1. If a value less than 1 is provided its ignored and set to 1.</value>
        **/
        public int MaxErrorCount {
            get => maxErrorCount;
            set{
                if(value > 1)maxErrorCount = value;
                else maxErrorCount = 1;
            }
        }
        public Flags(){
            PrintDebugInfo = true; //Print debug info
            OutputStream = System.Console.Out; //Use the default console output stream.
            MaxErrorCount = 1;
        }
    }
}