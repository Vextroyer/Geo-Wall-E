/*
This class receives an AST an executes the stamentes it represents.
*/

namespace Frontend;

public class Interpreter : Visitor<object?>{
    private Scope globalScope = new Scope();

    //Interpret a program.
    public object? Interpret (Program program){
        foreach(Stmt stmt in program.Stmts){
            Interpret(stmt,globalScope);
        }
        return null;
    }

    private object? Interpret (Stmt stmt,Scope scope){
        stmt.Accept(this,scope);
        return null;
    }
    public object? VisitPointStmt (Stmt.Point point,Scope scope){
        scope.Set(point.Id.Lexeme,new Element.Point(point.Id.Lexeme,point.X,point.Y,point.Comment));
        return null;
    }
    
    public object? VisitDrawStmt(Stmt.Draw draw, Scope scope)
    {
        scope.Get(draw.Id.Lexeme);
        System.Console.WriteLine("pinto");
        return null;
    }
}