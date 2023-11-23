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

    public Element VisitBinaryPowerExpr(Expr.Binary.Power powerExpr, Scope<Element> scope){
        Element.Number left = (Element.Number) Evaluate(powerExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(powerExpr.Right,scope);
        return left ^ right;
    }

    public Element VisitBinaryProductExpr(Expr.Binary.Product productExpr, Scope<Element> scope){
        Element.Number left = (Element.Number) Evaluate(productExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(productExpr.Right,scope);
        return left * right;
    }

    public Element VisitBinaryDivisionExpr(Expr.Binary.Division divisionExpr, Scope<Element> scope){
        Element.Number left = (Element.Number) Evaluate(divisionExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(divisionExpr.Right,scope);
        if(right.Value == 0.0f)throw new ExtendedException(divisionExpr.Line,divisionExpr.Operator.Offset,"Division by 0");
        return left / right;
    }

    public Element VisitBinaryModulusExpr(Expr.Binary.Modulus modulusExpr, Scope<Element> scope){
        Element.Number left = (Element.Number) Evaluate(modulusExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(modulusExpr.Right,scope);
        if(right.Value == 0.0f)throw new ExtendedException(modulusExpr.Line,modulusExpr.Operator.Offset,"Division by 0");
        return left % right;
    }

    public Element VisitBinarySumExpr(Expr.Binary.Sum sumExpr, Scope<Element> scope){
        Element.Number left = (Element.Number) Evaluate(sumExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(sumExpr.Right,scope);
        return left + right;
    }

    public Element VisitBinaryDifferenceExpr(Expr.Binary.Difference differenceExpr, Scope<Element> scope){
        Element.Number left = (Element.Number) Evaluate(differenceExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(differenceExpr.Right,scope);
        return left - right;
    }

    public Element VisitBinaryLessExpr(Expr.Binary.Less lessExpr, Scope<Element> scope){
        Element.Number left =(Element.Number) Evaluate(lessExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(lessExpr.Right,scope);
        return left < right;
    }

    public Element VisitBinaryLessEqualExpr(Expr.Binary.LessEqual lessEqualExpr, Scope<Element> scope){
        Element.Number left =(Element.Number) Evaluate(lessEqualExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(lessEqualExpr.Right,scope);
        return left <= right;
    }

    public Element VisitBinaryGreaterExpr(Expr.Binary.Greater greaterExpr, Scope<Element> scope){
        Element.Number left =(Element.Number) Evaluate(greaterExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(greaterExpr.Right,scope);
        return left > right;
    }

    public Element VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual greaterEqualExpr, Scope<Element> scope){
        Element.Number left =(Element.Number) Evaluate(greaterEqualExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(greaterEqualExpr.Right,scope);
        return left >= right;
    }

    public Element VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual equalEqualExpr, Scope<Element> scope){
        Element left = Evaluate(equalEqualExpr.Left,scope);
        Element right = Evaluate(equalEqualExpr.Right,scope);
        return left.EqualTo(right);
    }

    public Element VisitBinaryNotEqualExpr(Expr.Binary.NotEqual notEqualExpr, Scope<Element> scope){
        Element left = Evaluate(notEqualExpr.Left,scope);
        Element right = Evaluate(notEqualExpr.Right,scope);
        return left.NotEqualTo(right);
    }

    public Element VisitBinaryAndExpr(Expr.Binary.And andExpr, Scope<Element> scope){
        if(IsTruthy(Evaluate(andExpr.Left,scope)) == Element.FALSE)return Element.FALSE;//Shortcircuit
        return IsTruthy(Evaluate(andExpr.Right,scope));
    }

    public Element VisitBinaryOrExpr(Expr.Binary.Or orExpr, Scope<Element> scope){
        if(IsTruthy(Evaluate(orExpr.Left,scope)) == Element.TRUE)return Element.TRUE;//Shortcircuit
        return IsTruthy(Evaluate(orExpr.Right,scope));
    }

    public Element VisitConditionalExpr(Expr.Conditional conditionalExpr, Scope<Element> scope){
        if( IsTruthy(Evaluate(conditionalExpr.Condition,scope)) == Element.TRUE ) return Evaluate(conditionalExpr.ThenBranchExpr,scope);
        return Evaluate(conditionalExpr.ElseBranchExpr,scope);
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