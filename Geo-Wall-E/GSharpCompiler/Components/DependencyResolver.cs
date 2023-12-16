namespace GSharpCompiler;
///<summary>Takes a source code and returns a new source code with all dependencies resolved.</summary>
class DependencyResolver : GSharpCompilerComponent{
    ///<summary>Stores (fileName,sourceCode).</summary>
    Queue<(string,string)> filesToProcess = new Queue<(string,string)>();
    ///<summary>Associate to each fileName its dependencies and code.</summary>
    Dictionary<string,FileInfo> fileInfoTable = new Dictionary<string, FileInfo>();
    private string originalFile;
    private Compiler.Flags flags; 
    public DependencyResolver(string source, string fileName,ICollection<Error> errors,Compiler.Flags flags):base(flags.MaxErrorCount,errors){
        filesToProcess.Enqueue((fileName,source));
        originalFile = fileName;
        this.flags = flags;
    }
    public override void Abort()
    {
        throw new DependencyResolverException();
    }
    ///<summary>Entry method for the DependencyResolver.</summary>
    public List<Token> ScanAndResolveDependencies(){
        //While files to process exist.
        while(filesToProcess.Count > 0){
            string nameOfActualFile,sourceCode;
            (nameOfActualFile,sourceCode) = filesToProcess.Dequeue();//Retrieve the first element from the queue.
            
            if(fileInfoTable.ContainsKey(nameOfActualFile))continue;//If this file has been already processed ignore it.

            List<Token> tokens = (new Scanner(sourceCode,nameOfActualFile,MaxErrorCount,Errors)).Scan();//Scan the file.
            if(flags.PrintDebugInfo)Utils.PrintTokens(tokens,flags.OutputStream,nameOfActualFile);
            if(Errors.Count > 0)throw new ScannerException();//If Errors were found while scanning throw.
            
            List<string> imports = ParseImports(tokens);//Get the files that this file imports.
            if(Errors.Count > 0)throw new ParserException();
            fileInfoTable.Add(nameOfActualFile,new FileInfo(imports,tokens));//Put the info on the table.

            foreach(string fileToImport in imports){
                //Get the source code from the file.
                string sourceCodeOfFileToImport = "";
                try{
                    sourceCodeOfFileToImport = Utils.GetSourceFromFile(fileToImport);
                }catch(Exception e){
                    Errors.Add(new Error(-1,-1,nameOfActualFile,e.Message));
                    throw new DependencyResolverException();
                }

                filesToProcess.Enqueue((fileToImport,sourceCodeOfFileToImport));//Put the imported file on the queue.

                fileInfoTable[nameOfActualFile].DependsOn.Add(fileToImport);//Put it on the dependency graph.
            }
        }

        //Retrieve the order on which the files shuold be concatenated.
        List<string> filesReadyToConcat = new List<string>();
        try{
            filesReadyToConcat = Dfs();
        }catch(CircularDependenciesException e){
            //Circular dependencies are an unrecoverable error.
            OnErrorFound(-1,-1,e.File,e.Message,true);
        }

        //Concatenate the files and produce the program.
        List<Token> finalProgram = new List<Token>();
        //Concatenate the different programs.
        foreach(string file in filesReadyToConcat){
            foreach(Token token in fileInfoTable[file].Code){
                if(token.Type != TokenType.EOF)finalProgram.Add(token);//Ignore EOF on every other program.
            }
        }
        finalProgram.Add(fileInfoTable[originalFile].Code.Last());//Add a final EOF.

        return finalProgram;
    }
    ///<summary>Different states a node can be during the dfs.</summary>
    enum State{
        PROCESSED,
        PROCESSING,
        UNPROCESSED
    };
    ///<summary>Returns the order on which the files must be concatenated or throws if circular dependencies exist.</summary>
    List<string> Dfs(){
        Dictionary<string,State> state = new Dictionary<string, State>();//Mantain a state for each file.
        List<string> preOrder = new List<string>();//The order on which the files must be concatenated.
        Stack<string> callStack = new Stack<string>();//Used for printing cycles.
        ///<summary>Performs a dfs traversal on the dependencie graph.</summary>
        void Dfs(string file){
            callStack.Push(file);
            state[file] = State.PROCESSING;
            foreach(string toBeImported in fileInfoTable[file].DependsOn){
                switch(state[toBeImported]){
                    case State.UNPROCESSED:
                        Dfs(toBeImported);
                        break;
                    case State.PROCESSED:
                        //Ignore processed nodes.
                        break;
                    case State.PROCESSING:
                        //A cycle, circular dependencies.
                        string message = $"{toBeImported} <- ";
                        while(callStack.Peek() != toBeImported){
                            message += $"{callStack.Pop()} <- ";
                        }
                        message += toBeImported;
                        message = "The following dependencies are circular : " + message;
                        throw new CircularDependenciesException(toBeImported,message);
                }
            }
            preOrder.Add(file);
            state[file] = State.PROCESSED;
            callStack.Pop();
        }

        //Initialize states
        foreach(string file in fileInfoTable.Keys)state[file] = State.UNPROCESSED;
        Dfs(originalFile);//Performs a DFS on the originalFile

        return preOrder;
    }
    ///<summary>Takes a list of tokens and produce a list of import files. It uses help from the Parser. Can generate errors.</summary>
    private List<string> ParseImports(List<Token> tokens){
        List<string> imports = (new Parser(tokens,MaxErrorCount,Errors)).ParseImports();//Split the imports from the code.
        return imports;
    }

    ///<summary>Holds information about a file.</summary>
    private class FileInfo{
        ///<summary>Files imported by this file.</summary>
        public List<string> Imports {get; private set;}
        ///<summary>Code of this file.</summary>
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