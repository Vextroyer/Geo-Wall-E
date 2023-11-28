namespace GSharpCompiler;

///<summary>Base class for compiler components. <example>The parser</example>.</summary>
abstract class GSharpCompilerComponent : ICompileTimeErrorHandler{
    public int MaxErrorCount {get; protected set;}
    public ICollection<GSharpCompiler.Error> Errors {get; protected set;}
    ///<summary>Set<c>MaxErrorCount</c> and <c>Errors</c></summary>
    protected GSharpCompilerComponent(int maxErrorCount,ICollection<GSharpCompiler.Error> errors){
        MaxErrorCount = maxErrorCount;
        Errors = errors;
    }
    public virtual void HandleError(IAbortable caller,bool enforceAbort,int MaxErrorCount,ICollection<GSharpCompiler.Error> errors,string message,int line,int offset){
        errors.Add(new GSharpCompiler.Error(line,offset,message));//Add the new error to the collection.
        if(enforceAbort || errors.Count >= MaxErrorCount)caller.Abort();//If enforceAbort is set to true or the limit of error is reached ,abort.
    }
    public virtual void OnErrorFound(int line,int offset,string message,bool enforceAbort = false){
        HandleError(this,enforceAbort,this.MaxErrorCount,this.Errors,message,line,offset);
    }
    public abstract void Abort();
}