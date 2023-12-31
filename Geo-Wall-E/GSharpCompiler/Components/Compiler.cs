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
        Utils.SetMaxCoordinate(flags.MaxCoordinate);
        List<Error> errors = new List<Error>();
        List<IDrawable> elements = new List<IDrawable>();

        try
        {   
            //Scan the source code and produce the tokens.
            List<Token> tokens = new DependencyResolver(source,sourceFileName,errors,flags).ScanAndResolveDependencies();

            //If errors where found stop the compilation process.
            if(errors.Count > 0)throw new DependencyResolverException();

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
        ///<summary>Maximum coordinates to generate points.</summary>
        public int MaxCoordinate {
            get => maxCoordinate;
            set{
                if(value > 0)maxCoordinate = value;
            }
        }
        private int maxCoordinate;
        public Flags(){
            PrintDebugInfo = true; //Print debug info
            OutputStream = System.Console.Out; //Use the default console output stream.
            MaxErrorCount = 1;
            maxCoordinate = 1000;
        }
    }
}