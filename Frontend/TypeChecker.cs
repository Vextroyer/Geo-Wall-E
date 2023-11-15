/*
The type checker receives an asbtract syntax tree and checks that it
follows some rules.
*/

namespace Frontend;

/*
Type checking rules:
    1- Constants cant be redeclared. Therefore any instruccion that associates identifiers to values(create variables) must be checked to enforce this rule.
    2- Constants cant be assigned an empty expression.
    3- A variable cant be declared twice on the same scope.
    4-Variables must be declared before being used.
*/

public class TypeChecker : IVisitorStmt<object?,Element>, IVisitorExpr<ElementType,Element>
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
        scope.SetArgument(stmt.Id.Lexeme,new Element.Point(new Element.String(stmt.Id.Lexeme),stmt.X,stmt.Y,stmt.Comment));
        return null;
    }
    public object? VisitConstantDeclarationStmt(Stmt.ConstantDeclaration stmt,Scope<Element> scope){
        if(scope.IsConstant(stmt.Id.Lexeme))throw new ExtendedException(stmt.Line,stmt.Offset,$"Redeclaration of constant {stmt.Id.Lexeme}");//Rule 1
        if(stmt.Rvalue == Expr.EMPTY)throw new ExtendedException(stmt.Line,stmt.Offset,$"Assigned empty expression to constant `{stmt.Id.Lexeme}`");//Rule 2
        Check(stmt.Rvalue,scope);
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print stmt,Scope<Element> scope){
        Check(stmt._Expr,scope);
        return null;
    }
    //Checking expressions
    private void Check(Expr expr,Scope<Element> scope){
        expr.Accept(this,scope);
    }
    public ElementType VisitNumberExpr(Expr.Number expr,Scope<Element> scope){
        return ElementType.NUMBER;
    }
    public ElementType VisitStringExpr(Expr.String expr,Scope<Element> scope){
        return ElementType.STRING;
    }
    public ElementType VisitVariableExpr(Expr.Variable expr,Scope<Element> scope){
        if(!scope.HasBinding(expr.Id.Lexeme,true))throw new ExtendedException(expr.Line,expr.Offset,$"Variable `{expr.Id.Lexeme}` used but not declared");
        return scope.Get(expr.Id.Lexeme).Type;
    }
}