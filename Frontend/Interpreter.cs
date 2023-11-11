/*
This class receives an AST an executes the stamentes it represents.
*/

namespace Frontend;

public class Interpreter : Visitor<object?>{
    private Scope scope = new Scope();

    //Interpret a program.
    public object? Interpret (Program program){
        foreach(Stmt stmt in program.Stmts){
            Interpret(stmt);
        }
        return null;
    }

    private object? Interpret (Stmt stmt){
        stmt.Accept(this);
        return null;
    }
    public object? VisitPointStmt (Stmt.Point point){
        scope.Set(point.Id.Lexeme,new Point(point.Id.Lexeme,point.Comment));
        return null;
    }
}