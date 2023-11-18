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
    public object? VisitPointStmt(Stmt.Point pointStmt,Scope<Element> scope){
        if(scope.IsConstant(pointStmt.Id.Lexeme))throw new ExtendedException(pointStmt.Line,pointStmt.Offset,$"Redeclaration of constant {pointStmt.Id.Lexeme}");//Rule 1
        if(scope.HasBinding(pointStmt.Id.Lexeme))throw new ExtendedException(pointStmt.Line,pointStmt.Offset,$"Point `{pointStmt.Id.Lexeme}` is declared twice on the same scope");//Rule 3
        scope.SetArgument(pointStmt.Id.Lexeme,Element.POINT);
        return null;
    }
    public object? VisitConstantDeclarationStmt(Stmt.ConstantDeclaration declStmt,Scope<Element> scope){
        if(scope.IsConstant(declStmt.Id.Lexeme))throw new ExtendedException(declStmt.Line,declStmt.Offset,$"Redeclaration of constant {declStmt.Id.Lexeme}");//Rule 1
        Element rValue = Check(declStmt.Rvalue,scope);
        scope.SetConstant(declStmt.Id.Lexeme,rValue);
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print printStmt,Scope<Element> scope){
        Check(printStmt._Expr,scope);
        return null;
    }
    public object? VisitColorStmt(Stmt.Color colorStmt,Scope<Element> scope){
        //A color statement doesnt invloves any checking.
        return null;
    }
    public object? VisitDrawStmt(Stmt.Draw drawStmt,Scope<Element> scope)
    {
        Element element = Check(drawStmt._Expr,scope);
        //element is of class Element, but its an instance of a subclass of Element, some of which implements the interface IDrawable. Rule # 5
        if(!(element is IDrawable))throw new ExtendedException(drawStmt._Expr.Line,drawStmt._Expr.Offset,$"Element of type `{element.Type}` is not drawable");
        return null;
    }
    //Checking expressions
    private Element Check(Expr expr,Scope<Element> scope){
        return expr.Accept(this,scope);
    }
    public Element VisitNumberExpr(Expr.Number numberExpr,Scope<Element> scope){
        return Element.NUMBER;
    }
    public Element VisitStringExpr(Expr.String stringExpr,Scope<Element> scope){
        return Element.STRING;
    }
    public Element VisitVariableExpr(Expr.Variable variableExpr,Scope<Element> scope){
        if(!scope.HasBinding(variableExpr.Id.Lexeme,true))throw new ExtendedException(variableExpr.Line,variableExpr.Offset,$"Variable `{variableExpr.Id.Lexeme}` used but not declared");//Rule 4
        return scope.Get(variableExpr.Id.Lexeme);
    }
    public Element VisitUnaryNotExpr(Expr.Unary.Not unaryNotExpr, Scope<Element> scope){
        Check(unaryNotExpr._Expr,scope);//Check the right hand expr.
        return Element.NUMBER;//Rule # 6
    }
    public Element VisitUnaryMinusExpr(Expr.Unary.Minus unaryMinusExpr, Scope<Element> scope){
        //Check the right hand expr.
        //Rule # 7
        Element rValue = Check(unaryMinusExpr._Expr,scope);
        if(rValue.Type != ElementType.NUMBER)throw new ExtendedException(unaryMinusExpr.Line,unaryMinusExpr.Offset,$"Applied `-` operator to a {rValue.Type} operand");
        return Element.NUMBER;
    }
    public Element VisitBinaryPowerExpr(Expr.Binary.Power powerExpr, Scope<Element> scope){
        Element operand = Check(powerExpr.Left,scope);//Check left operand
        if(operand.Type != ElementType.NUMBER)throw new ExtendedException(powerExpr.Left.Line,powerExpr.Left.Offset,$"Left operand of {powerExpr.Operator.Lexeme} is {operand.Type} and must be NUMBER");
        operand = Check(powerExpr.Right,scope);//Check right operand
        if(operand.Type != ElementType.NUMBER)throw new ExtendedException(powerExpr.Right.Line,powerExpr.Right.Offset,$"Right operand of {powerExpr.Operator.Lexeme} is {operand.Type} and must be NUMBER");
        return Element.NUMBER;
    }
}