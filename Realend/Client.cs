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
            
            string? source;
            try{
                source = GetSourceFromFile(path);
            }catch(FileNotFoundException e){
                ReportError(e.Message);
                continue;
            }
            //At this point on source its the content of the G# script to be executed.
            Frontend.GSharpCompiler.Response compilerResponse = Frontend.GSharpCompiler.CompileFromSource(source);
            if(compilerResponse.HadError){
                foreach(Frontend.GSharpCompiler.Error error in compilerResponse.Errors)
                {
                    ReportError(error.Line,error.Offset,error.Message);
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

    //Returns the contents of a file as an string. Its used to retrieve the content of a file. Throws an error if the file cant be oppened.
    private static string? GetSourceFromFile(string path){
        path = Path.GetFullPath(path);//Convert the path to absolute path, this is to support relative paths

        //The file doesnt exist, this is an error.
        if(!Path.Exists(path))throw new FileNotFoundException("Error accessing file. Check if exists and that this program can read that file.");

        // byte[] bytes = File.ReadAllBytes(path);//Read the file content
        // string source = System.Text.Encoding.Default.GetString(bytes);//Convert binary content on its textual representation
        // return source;

        return System.Text.Encoding.Default.GetString(File.ReadAllBytes(path));
    }
}
