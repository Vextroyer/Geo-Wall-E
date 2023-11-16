/*
This class acts as an intermediary between the user interface and the compiler.
It receives G# source code and produces drawable objects. This objects must be handled
by the specific drawing interface. If the compilation fails it return the errors occurred
at compile time.
*/

namespace Frontend;

public static class GSharpCompiler
{
    //Compile from a string containing the source code.
    public static Response CompileFromSource(string? source,Flags? flags = null)
    {
        //Use default flags
        if(flags == null)flags = new Flags();

        List<Error> errors = new List<Error>();
        List<IDrawable> elements = new List<IDrawable>();

        try
        {   
            //Scan the source code and produce the tokens.
            List<Token> tokens = new Scanner(source).Scan();

            //Print scnanner output
            if(flags.PrintDebugInfo)Utils.PrintTokens(tokens,flags.OutputStream);

            //Parse the tokens into an abstract syntax tree and store the tree.
            Program program = new Parser(tokens).Parse();

            //Print parser output
            if(flags.PrintDebugInfo)Utils.PrintAst(program,flags.OutputStream);

            //Check the semantic of the program.
            new TypeChecker().Check(program);

            //Interpret the program to produce drawable objects.
            elements = new Interpreter(flags.OutputStream).Interpret(program);
        }
        catch (Frontend.ExtendedException e)
        {
            errors.Add(ExtendedExceptionToError(e));
        }
        catch (Exception)
        {
            //Do something on unexpected exceptions catched.
        }

        return new Response(errors,elements);
    }
    //Data structure to hold the objects produced by the compilation process and the errors.
    public class Response
    {
        public bool HadError { get => errors.Count > 0; }
        public List<Error> Errors { get => errors; }
        public List<IDrawable> Elements { get => elements; }
        List<Error> errors = new List<Error>();
        List<IDrawable> elements = new List<IDrawable>();//The elements produced by the code.
        public Response(List<Error> _errors, List<IDrawable> _elements)
        {
            errors = _errors;
            elements = _elements;
        }
    }
    //Represents the errors.
    public class Error
    {
        public int Line { get; private set; }
        public int Offset { get; private set; }
        public string Message { get; private set; }
        public Error(int line, int offset, string message)
        {
            Line = line;
            Offset = offset;
            Message = message;
        }
    }
    //Transforms an ExtendedException onto an Error. Remove on the future and pass a way to collect errors.
    private static Error ExtendedExceptionToError(ExtendedException e)
    {
        return new Error(e.Line, e.Offset, e.Message);
    }
    //Represents several options that alter the behaviour of the compiler.
    public class Flags{
        //This flags determine wheter the compiler should print the tokens produced by the scanner and the ast produced by the parser.
        public bool PrintDebugInfo {get; set;}
        //This flag is for setting the stream to which the client application wants to redirect the output of the `print` statements and the debug info.
        public TextWriter OutputStream {get; private set;}
        public Flags(){
            PrintDebugInfo = true; //Print debug info
            OutputStream = System.Console.Out; //Use the default console output stream.
        }
    }
}