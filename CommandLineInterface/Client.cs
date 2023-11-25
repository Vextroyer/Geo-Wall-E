/*
This is a CLI to the G# compiler.
*/
namespace Realend;

using GSharpCompiler;

class Client{
    public static void Main(string[] args){
        PrintWelcome();
        SelectMode(args);
    }

    private static void PrintWelcome(){
        Console.Title = "Wall-E : a DaVinci on G#";
        Console.WriteLine("Hello user. I am Wall-E and i will be your assistant during this journey.");
        Console.WriteLine("Press Ctrl+C at any time to stop execution.");
        Console.WriteLine();
    }

    /*
    Provides support for different modes of using the application.
    */
    private static void SelectMode(string[] args){
        GSharpCompiler.Compiler.Flags flags = new GSharpCompiler.Compiler.Flags();
        if(args.Contains("--noDebug"))flags.PrintDebugInfo = false;
        if(args.Contains("--SetMaxErrorCount"))flags.MaxErrorCount = 5;//Use 5 errors by default.
        if(args.Contains("--runRepl"))RunREPL(flags);
        else if(args.Contains("--runTest"))RunTest(flags);
        else RunFromFile(flags);//Default behaviour.
    }
    /*
    This mode establish a REPL session with the user.
    */
    private static void RunREPL(Compiler.Flags flags){
        while(true){
            Console.Write("> ");
            string? source = Console.ReadLine();//The code inputed by the user.
            if(string.IsNullOrEmpty(source))continue;//User just pressed enter or hit to many spaces.
            Compiler.Response compilerResponse = Compiler.CompileFromSource(source,flags);
            HandleError(compilerResponse);
        }
    }
    /*
    This mode asks user to input a file and then executes the script on the file and shows the output.
    */
    private static void RunFromFile(Compiler.Flags flags){
        Console.WriteLine("Provide files to execute");
        while(true){
            Console.Write("> ");
            string? path = Console.ReadLine();//The path to the file
            if(string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))continue;//User just pressed enter or hit to many spaces.
            
            Compiler.Response compilerResponse = Compiler.CompileFromFile(path,flags);
            HandleError(compilerResponse);
        }
    }
    /*
    This mode sends the `test.gs` file to the compiler and redirects its output to the `test.out` file, then compares the results
    from `test.out` with the supplied results at `test.results` and outputs the results.
    */
    private static void RunTest(GSharpCompiler.Compiler.Flags flags){
        flags.PrintDebugInfo = false;//You dont want debug info missing up with the results.
        
        //Paths are relative to the CommandLineInterface parent folder.        
        string testGs = "CommandLineInterface/Testing/test.gs";//Source code
        string testOut = "CommandLineInterface/Testing/test.out";//Computed results
        string testResult = "CommandLineInterface/Testing/test.result";//Expected results

        flags.OutputStream = File.CreateText(testOut);//Set test.out to be the output stream.

        // Compile and run test.gs file, the output of print statements is written on test.out file
        Compiler.Response compilerResponse =  Compiler.CompileFromFile(testGs,flags);
        HandleError(compilerResponse);
        //Flush the output stream
        flags.OutputStream.Flush();

        StreamReader expectedResultsFile = File.OpenText(testResult);
        StreamReader resultsFile = File.OpenText(testOut);

        int test = 1;
        while(!expectedResultsFile.EndOfStream && !resultsFile.EndOfStream){
            Console.Write($"T {test}:   ");
            string result = resultsFile.ReadLine()!;
            string expectedResult = expectedResultsFile.ReadLine()!;
            if(result == expectedResult){
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else{
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Expected \"{expectedResult}\" but found \"{result}\"");
                Console.ForegroundColor = ConsoleColor.White;
            }
            ++test;
        }
        if(expectedResultsFile.EndOfStream && resultsFile.EndOfStream)Console.WriteLine("Test finished.");
        else Console.WriteLine("Number of tests does not concords with the number of results.");
    }
    //Returns true if the compiler detected any error.
    private static bool HandleError(Compiler.Response compilerResponse){
        if(compilerResponse.HadError){
            if(compilerResponse.HadErrorReadingFile)ReportError(compilerResponse.Errors.Last().Message);
            else
            {
                foreach(Error error in compilerResponse.Errors)
                {
                    ReportError(error.Line,error.Offset,error.Message);
                }
            }
            return true;
        }
        return false;
    }
    private static void ReportError(string message){
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
    }
    private static void ReportError(int line,int offset,string message){
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{message} at line {line} , column {offset}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
