/*
The client application handles user requests on what programs to execute and
displays the visual output.
*/
namespace Realend;

using Frontend;

class Client{
    public static void Main(){
        PrintWelcome();
        SelectMode();
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
    private static void SelectMode(){
        RunFromFile();
    }

    /*
    This mode asks user to input a file and then executes the script on the file and shows the output.
    */
    private static void RunFromFile(){
        Console.WriteLine("Provide files to execute");
        while(true){
            Console.Write("> ");
            string? path = Console.ReadLine();//The path to the file
            if(string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))continue;//User just pressed enter or hit to many spaces.
            
            //At this point on source its the content of the G# script to be executed.
            Frontend.GSharpCompiler.Response compilerResponse = Frontend.GSharpCompiler.CompileFromFile(path);
            if(compilerResponse.HadError){
                if(compilerResponse.HadErrorReadingFile)ReportError(compilerResponse.Errors.Last().Message);
                else
                {
                    foreach(Frontend.GSharpCompiler.Error error in compilerResponse.Errors)
                    {
                        ReportError(error.Line,error.Offset,error.Message);
                    }
                }
            }
        }
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
