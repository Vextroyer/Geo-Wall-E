/*
Define exceptions to be used by the program.
*/
namespace Frontend;


///<summary>Abort excecution by throwing an excpetion.</summary>
interface IAbortable
{
    ///<summary>Throws an exception upon calling this method.</summary>
    public void Abort() ;
}
///<summary></summary>
interface ICompileTimeErrorHandler : IAbortable{
    ///<summary>Maximum amount of errors to be detected before aborting.</summary>
    public int MaxErrorCount {get;}
    ///<summary>Returns the detected errors.</summary>
    public ICollection<GSharpCompiler.Error> Errors {get;}
    ///<summary>Add a new error with the supplied information to the list of errors and abort the excecution if the amount of errors detected is greater than or equal to <c>MaxErrorCount</c>. If <c>enforceAbort</c> is set to true aborts always abort.</summary>
    ///<param name="caller">The class that called this method</param>
    ///<param name="enforceAbort">Wheter this method should abort unconditionally. For example if an unrecoverable error was found.</param>
    ///<param name="MaxErrorCount">The maximum amount of errors to detect before aborting.</param>
    ///<param name="errors">A collection containing the errors.</param>
    ///<param name="message">The message of the error.</param>
    ///<param name="line">The line where the error was found.</param>
    ///<param name="offset">An approximate location of the column where the error was found.</param>
    public void HandleError(IAbortable caller,bool enforceAbort,int MaxErrorCount,ICollection<GSharpCompiler.Error> errors,string message,int line,int offset);
    ///<summary>Simplified call to equal <c>HandleError</c> method.</summary>
    public void ReportError(int line,int offset,string message,bool enforceAbort = false);
}

///<summary>Base class for all compiler defined exceptions.</summary>
abstract class GSharpException : Exception{}
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

class ExtendedException : Exception{
    public int Line {get; private set;}
    public int Offset { get; private set;}
    public ExtendedException(int line,int offset,string message):base(message){
        Line = line;
        Offset=offset;
    }
}