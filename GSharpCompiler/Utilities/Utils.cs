namespace GSharpCompiler;
///<summary>Utility methods for namespace classes.</summary>
static class Utils{
    ///<summary>Print the output of the Scanner.</summary>
    ///<param name="tokens">The scanner output</param>
    ///<param name="outputStream">A stream to print to.</param>
    public static void PrintTokens(List<Token> tokens, TextWriter outputStream,string file = ""){
        outputStream.WriteLine($"Scanner output {file} : Begin");
        outputStream.WriteLine("{");
        foreach(Token t in tokens)outputStream.WriteLine("\t" + t);
        outputStream.WriteLine("}");
        outputStream.WriteLine("Scanner output : End");
    }
    ///<summary>Print the output of the Parser</summary>
    ///<param name="program">The parser output.</param>
    ///<param name="outputStream">A stream to print to.</param>
    public static void PrintAst(Program program, TextWriter outputStream){
        bool usingConsole = outputStream == Console.Out;
        AstPrinter printer = new AstPrinter();

        if(usingConsole) Console.ForegroundColor = ConsoleColor.Yellow;
        outputStream.WriteLine($"The program has {program.Stmts.Count} statements.");
        outputStream.WriteLine("--START--");
        if(usingConsole) Console.ForegroundColor = ConsoleColor.White;

        foreach(Stmt stmt in program.Stmts){
            Console.WriteLine(printer.Print(stmt));
        }

        if(usingConsole)Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--END--");
        if(usingConsole)Console.ForegroundColor = ConsoleColor.White;
    }

    //Random point generation
    private static Random random = new Random();//Stores a random for future usage.
    public static void SetMaxCoordinate(int maxCoordinate){
        MaxPointCoordinate = maxCoordinate;
    }
    private static int MaxPointCoordinate = 1000;//Represents the maximum absolute value of point coordinates
    ///<summary>Generate an integer on the range [-MaxPointCoordinate,MaxPointCoordinate]</summary>
    public static int RandomCoordinate(){
        return (random.Next() % (2 * MaxPointCoordinate + 1)) - MaxPointCoordinate;
    }

    ///<summary>Returns the contents of a file as an string. Its used to retrieve the content of a file. Throws an error if the file cant be oppened.</summary>
    ///<param name="path">The path to the file.</summary>
    public static string GetSourceFromFile(string path){
        path = Path.GetFullPath(path);//Convert the path to absolute path, this is to support relative paths

        //The file doesnt exist, this is an error.
        if(!Path.Exists(path))throw new FileNotFoundException(path);

        // byte[] bytes = File.ReadAllBytes(path);//Read the file content
        // string source = System.Text.Encoding.Default.GetString(bytes);//Convert binary content on its textual representation
        // return source;

        return System.Text.Encoding.Default.GetString(File.ReadAllBytes(path));
    }

    public static void AppendToLog(Exception e){
        string logMessage = $"\n{System.DateTime.Now}\n{e.Message}\n{e.StackTrace}\n";
        File.AppendAllText("log",logMessage);
    }
}