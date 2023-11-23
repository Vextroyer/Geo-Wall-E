/*
Several utility methods and features.
*/

namespace Frontend;

static class Utils{
    //Used for printing the output of the Scanner
    public static void PrintTokens(List<Token> tokens, TextWriter outputStream){
        outputStream.WriteLine("{");
        foreach(Token t in tokens)outputStream.WriteLine("\t" + t);
        outputStream.WriteLine("}");
    }
    //Used for printing the output of the Parser
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
    private static int MaxPointCoordinate = 1000;//Represents the maximum absolute value of point coordinates
    //Generate an integer on the range [-MaxPointCoordinate,MaxPointCoordinate]
    public static int RandomCoordinate(){
        return (random.Next() % (2 * MaxPointCoordinate + 1)) - MaxPointCoordinate;
    }

    //Returns the contents of a file as an string. Its used to retrieve the content of a file. Throws an error if the file cant be oppened.
    public static string GetSourceFromFile(string path){
        path = Path.GetFullPath(path);//Convert the path to absolute path, this is to support relative paths

        //The file doesnt exist, this is an error.
        if(!Path.Exists(path))throw new FileNotFoundException();

        // byte[] bytes = File.ReadAllBytes(path);//Read the file content
        // string source = System.Text.Encoding.Default.GetString(bytes);//Convert binary content on its textual representation
        // return source;

        return System.Text.Encoding.Default.GetString(File.ReadAllBytes(path));
    }
}