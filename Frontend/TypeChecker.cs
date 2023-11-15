/*
The type checker receives an asbtract syntax tree and checks that it
follows some rules.
*/

namespace Frontend;

public class TypeChecker : IVisitorStmt<object?,ElementType>, IVisitorExpr<ElementType,ElementType>
{
    Scope<ElementType> globalScope = new Scope<ElementType>();
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
    public object? VisitPointStmt(Stmt.Point stmt,Scope<ElementType> scope){
        if(scope.IsConstant(stmt.Id.Lexeme))throw new ExtendedException(stmt.Line,stmt.Offset,$"Redeclaration of constant {stmt.Id.Lexeme}");//Rule 1
        if(scope.HasBinding(stmt.Id.Lexeme))throw new ExtendedException(stmt.Line,stmt.Offset,$"Point `{stmt.Id.Lexeme}` is declared twice on the same scope");//Rule 3
        scope.SetArgument(stmt.Id.Lexeme,ElementType.POINT);
        return null;
    }
    public object? VisitConstantDeclarationStmt(Stmt.ConstantDeclaration stmt,Scope<ElementType> scope){
        if(scope.IsConstant(stmt.Id.Lexeme))throw new ExtendedException(stmt.Line,stmt.Offset,$"Redeclaration of constant {stmt.Id.Lexeme}");//Rule 1
        ElementType rValueType = Check(stmt.Rvalue,scope);
        scope.SetConstant(stmt.Id.Lexeme,rValueType);
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print stmt,Scope<ElementType> scope){
        Check(stmt._Expr,scope);
        return null;
    }
    public object? VisitColorStmt(Stmt.Color stmt,Scope<ElementType> scope){
        //A color statement doesnt invloves any checking.
        return null;
    }
    //Checking expressions
    private ElementType Check(Expr expr,Scope<ElementType> scope){
        return expr.Accept(this,scope);
    }
    public ElementType VisitNumberExpr(Expr.Number expr,Scope<ElementType> scope){
        return ElementType.NUMBER;
    }
    public ElementType VisitStringExpr(Expr.String expr,Scope<ElementType> scope){
        return ElementType.STRING;
    }
    public ElementType VisitVariableExpr(Expr.Variable expr,Scope<ElementType> scope){
        if(!scope.HasBinding(expr.Id.Lexeme,true))throw new ExtendedException(expr.Line,expr.Offset,$"Variable `{expr.Id.Lexeme}` used but not declared");//Rule 4
        return scope.Get(expr.Id.Lexeme);
    }
}