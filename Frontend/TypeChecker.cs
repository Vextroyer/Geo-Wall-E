/*
The type checker receives an asbtract syntax tree and checks that it
follows some rules.
The type checker doesnt create new elements, it just uses the constants
provided on the Element class to represent the objects.
*/

namespace Frontend;

class TypeChecker : IVisitorStmt<object?,Element>, IVisitorExpr<Element,Element>
{
    Scope<Element> globalScope = new Scope<Element>();
    //Analyze the semantic of the program to see if it is correct.
    public void Check(Program program)
    {
        foreach (Stmt stmt in program.Stmts)
        {
            Check(stmt);
        }
    }
    private void Check(Stmt stmt)
    {
        stmt.Accept(this, globalScope);
    }
    //Checking statements
    public object? VisitPointStmt(Stmt.Point stmt,Scope<Element> scope){
        if(scope.IsConstant(stmt.Id.Lexeme))throw new ExtendedException(stmt.Line,stmt.Offset,$"Redeclaration of constant {stmt.Id.Lexeme}");//Rule 1
        if(scope.HasBinding(stmt.Id.Lexeme))throw new ExtendedException(stmt.Line,stmt.Offset,$"Point `{stmt.Id.Lexeme}` is declared twice on the same scope");//Rule 3
        scope.SetArgument(stmt.Id.Lexeme,Element.POINT);
        return null;
    }
    public object? VisitConstantDeclarationStmt(Stmt.ConstantDeclaration stmt,Scope<Element> scope){
        if(scope.IsConstant(stmt.Id.Lexeme))throw new ExtendedException(stmt.Line,stmt.Offset,$"Redeclaration of constant {stmt.Id.Lexeme}");//Rule 1
        Element rValue = Check(stmt.Rvalue,scope);
        scope.SetConstant(stmt.Id.Lexeme,rValue);
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print stmt,Scope<Element> scope){
        Check(stmt._Expr,scope);
        return null;
    }
    public object? VisitColorStmt(Stmt.Color stmt,Scope<Element> scope){
        //A color statement doesnt invloves any checking.
        return null;
    }
    public object? VisitDrawStmt(Stmt.Draw stmt,Scope<Element> scope)
    {
        Element element = Check(stmt._Expr,scope);
        //element is of class Element, but its an instance of a subclass of Element, some of which implements the interface IDrawable. Rule # 5
        if(!(element is IDrawable))throw new ExtendedException(stmt._Expr.Line,stmt._Expr.Offset,$"Element of type `{element.Type}` is not drawable");
        return null;
    }
    //Checking expressions
    private Element Check(Expr expr,Scope<Element> scope){
        return expr.Accept(this,scope);
    }
    public Element VisitNumberExpr(Expr.Number expr,Scope<Element> scope){
        return Element.NUMBER;
    }
    public Element VisitStringExpr(Expr.String expr,Scope<Element> scope){
        return Element.STRING;
    }
    public Element VisitVariableExpr(Expr.Variable expr,Scope<Element> scope){
        if(!scope.HasBinding(expr.Id.Lexeme,true))throw new ExtendedException(expr.Line,expr.Offset,$"Variable `{expr.Id.Lexeme}` used but not declared");//Rule 4
        return scope.Get(expr.Id.Lexeme);
    }
}