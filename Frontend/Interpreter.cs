/*
This class receives an AST an executes the stamentes it represents.
*/

namespace Frontend;

class Interpreter : IVisitorStmt<object?, Element>, IVisitorExpr<Element, Element>
{
    //Store the state of the program as a nested scope structure
    private Scope<Element> globalScope = new Scope<Element>();

    //Store the colors to draw
    private ColorStack colorStack = new ColorStack();

    //Stores the drawables
    private List<IDrawable> drawables = new List<IDrawable>();//On execution of a draw statement drawables must be added to the list.

    //Stores the stream to put `print` output
    private TextWriter outputStream;

    public Interpreter(TextWriter _outputStream)
    {
        outputStream = _outputStream;
    }

    //Interpret a program.
    public List<IDrawable> Interpret(Program program)
    {
        foreach (Stmt stmt in program.Stmts)
        {
            Interpret(stmt, globalScope);
        }
        return drawables;
    }
    #region Interpret statements
    private object? Interpret(Stmt stmt, Scope<Element> scope)
    {
        stmt.Accept(this, scope);
        return null;
    }
    public object? VisitPointStmt(Stmt.Point point, Scope<Element> scope)
    {
        scope.SetArgument(point.Id.Lexeme, new Element.Point(new Element.String(point.Id.Lexeme), point.X, point.Y, point.Comment));
        return null;
    }
    public object? VisitConstantDeclarationStmt(Stmt.ConstantDeclaration declaration, Scope<Element> scope)
    {
        scope.SetConstant(declaration.Id.Lexeme, Evaluate(declaration.Rvalue, scope));
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print stmt, Scope<Element> scope)
    {
        outputStream.WriteLine(Evaluate(stmt._Expr, scope));
        return null;
    }
    public object? VisitColorStmt(Stmt.Color stmt, Scope<Element> scope)
    {
        if (stmt.IsRestore) colorStack.Pop();
        else colorStack.Push(stmt._Color);
        return null;
    }

    public object? VisitDrawStmt(Stmt.Draw stmt, Scope<Element> scope)
    {
        IDrawable drawableElement = (IDrawable)Evaluate(stmt._Expr, scope);
        drawables.Add(drawableElement);
        // Esto lleva arreglo futuro
        return null;
    }

    #endregion Interpret statements

    #region Interpret expressions
    //Evaluate expressionss
    public Element Evaluate(Expr expr, Scope<Element> scope)
    {
        return expr.Accept(this, scope);
    }

    public Element VisitNumberExpr(Expr.Number expr, Scope<Element> scope)
    {
        return expr.Value;
    }

    public Element VisitStringExpr(Expr.String expr, Scope<Element> scope)
    {
        return expr.Value;
    }

    public Element VisitVariableExpr(Expr.Variable expr, Scope<Element> scope)
    {
        return scope.Get(expr.Id.Lexeme);
    }

    public Element VisitUnaryNotExpr(Expr.Unary.Not unaryNot, Scope<Element> scope)
    {
        Element rValue = Evaluate(unaryNot._Expr, scope);
        return IsNotTruty(rValue);
    }

    public Element VisitUnaryMinusExpr(Expr.Unary.Minus unaryMinus, Scope<Element> scope){
        Element.Number rValue = (Element.Number) Evaluate(unaryMinus._Expr,scope);
        return -rValue;
    }
    #endregion Interpret expressions

    //Determine if the given element is true or false. Undefined and 0 are false, everything else is true.
    private Element.Number IsTruthy(Element element)
    {
        switch (element.Type)
        {
            case ElementType.NUMBER:
                if (((Element.Number)element).Value != 0f) return Element.TRUE;
                return Element.FALSE;
            case ElementType.UNDEFINED:
                return Element.FALSE;
            default:
                return Element.TRUE;
        }
    }
    //Returns the opposite truth value of the given element.
    private Element.Number IsNotTruty(Element element){
        Element.Number truthValue = IsTruthy(element);
        if(truthValue == Element.TRUE)return Element.FALSE;
        return Element.TRUE;
    }
}