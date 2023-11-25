namespace GSharpCompiler;

///<summary>Base class for all compiler defined exceptions.</summary>
abstract class GSharpException : System.Exception{}
///<summary>Base class for compile-time exceptions.</summary>
abstract class CompileTimeException : GSharpException{}
///<summary>Base class for runtime exceptions.</summary>
abstract class RuntimeException : GSharpException{}

//Exceptions for each of the components of the compiler.

///<summary>Exception throwed by the scanner to signal errors.</summary>
class ScannerException : CompileTimeException{}
///<summary>Exception throwed by the parser to signal errors.</summary>
class ParserException : CompileTimeException{}
///<summary>Exception throwed by the type checker to signal errors.</summary>
class TypeCheckerException : CompileTimeException{}
///<summary>Exception throwed by the interpreter to signal errors.</summary>
class InterpreterException : RuntimeException{}

class ExtendedException : System.Exception{
    public int Line {get; private set;}
    public int Offset { get; private set;}
    public ExtendedException(int line,int offset,string message):base(message){
        Line = line;
        Offset=offset;
    }
}