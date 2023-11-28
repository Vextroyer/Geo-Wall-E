namespace GSharpCompiler;

///<summary>A G# program.</summary>
class Program{
    public Stmt.StmtList Stmts {get; private set;}
    public Program(Stmt.StmtList stmts){
        Stmts = stmts;
    }
}