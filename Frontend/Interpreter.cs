/*
This class receives an AST an executes the stamentes it represents.
*/

namespace Frontend;

public class Interpreter : IVisitorStmt<object?,Element>, IVisitorExpr<Element,Element>{
    private Scope<Element> globalScope = new Scope<Element>();

    //Interpret a program.
    public object? Interpret (Program program){
        foreach(Stmt stmt in program.Stmts){
            Interpret(stmt,globalScope);
        }
        return null;
    }
    #region Interpret statements
    private object? Interpret (Stmt stmt,Scope<Element> scope){
        stmt.Accept(this,scope);
        return null;
    }
    public object? VisitPointStmt (Stmt.Point point,Scope<Element> scope){
        scope.SetArgument(point.Id.Lexeme,new Element.Point(new Element.String(point.Id.Lexeme),point.X,point.Y,point.Comment));
        return null;
    }
    public object? VisitConstantDeclarationStmt (Stmt.ConstantDeclaration declaration,Scope<Element> scope){
        scope.SetConstant(declaration.Id.Lexeme,Evaluate(declaration.Rvalue,scope));
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print stmt,Scope<Element> scope){
        Console.WriteLine(Evaluate(stmt._Expr,scope));
        return null;
    }
    #endregion Interpret statements

    #region Interpret expressions
    //Evaluate expressionss
    public Element Evaluate(Expr expr,Scope<Element> scope){
        return expr.Accept(this,scope);
    }

    public Element VisitNumberExpr(Expr.Number expr,Scope<Element> scope){
        return expr.Value;
    }

    public Element VisitStringExpr(Expr.String expr,Scope<Element> scope){
        return expr.Value;
    }

    public Element VisitVariableExpr(Expr.Variable expr,Scope<Element> scope){
        return scope.Get(expr.Id.Lexeme);
    }
    #endregion Interpret expressions
}