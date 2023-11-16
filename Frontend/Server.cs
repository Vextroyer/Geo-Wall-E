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
    public static Response CompileFromSource(string? source)
    {
        List<Error> errors = new List<Error>();
        List<IDrawable> elements = new List<IDrawable>();
        try
        {
            List<Token> tokens = new Scanner(source).Scan();
            Parser parser = new Parser(tokens);
            Program program = parser.Parse();
            TypeChecker checker = new TypeChecker();
            checker.Check(program);
            Interpreter interpreter = new Interpreter();
            elements = interpreter.Interpret(program);
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
    //Transforms an ExtendedException onto an Error.
    private static Error ExtendedExceptionToError(ExtendedException e)
    {
        return new Error(e.Line, e.Offset, e.Message);
    }
}