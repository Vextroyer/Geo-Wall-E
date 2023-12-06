namespace GSharpCompiler;

///<summary>Abort excecution by throwing an excpetion.</summary>
interface IAbortable
{
    ///<summary>Throws an exception upon calling this method.</summary>
    public void Abort() ;
}

///<summary>Stablish an error handling behaviour for compile time errors for different compiler components. Works in conjunction with the <c>CompilerComponent</c> class.</summary>
interface ICompileTimeErrorHandler : IAbortable{
    ///<summary>Maximum amount of errors to be detected before aborting.</summary>
    public int MaxErrorCount {get;}
    
    ///<summary>Returns the detected errors.</summary>
    public ICollection<Error> Errors {get;}

    ///<summary>Add a new error with the supplied information to the list of errors and abort the excecution if the amount of errors detected is greater than or equal to <c>MaxErrorCount</c>. If <c>enforceAbort</c> is set to true aborts always abort.</summary>
    ///<param name="caller">The class that called this method</param>
    ///<param name="enforceAbort">Wheter this method should abort unconditionally. For example if an unrecoverable error was found.</param>
    ///<param name="MaxErrorCount">The maximum amount of errors to detect before aborting.</param>
    ///<param name="errors">A collection containing the errors.</param>
    ///<param name="message">The message of the error.</param>
    ///<param name="line">The line where the error was found.</param>
    ///<param name="offset">An approximate location of the column where the error was found.</param>
    public void HandleError(IAbortable caller,bool enforceAbort,int MaxErrorCount,ICollection<GSharpCompiler.Error> errors,string message,int line,int offset,string file);
    
    ///<summary>Simplified call to <c>HandleError</c> method.</summary>
    public void OnErrorFound(IErrorLocalizator error,string message,bool enforceAbort = false);
    public void OnErrorFound(int line,int offset,string file,string message,bool enforceAbort = false);
}
///<summary>Contains information on the location of the error.</summary>
interface IErrorLocalizator{
    public int Line {get ;}
    public int Offset {get; }
    public string File {get; }
}

