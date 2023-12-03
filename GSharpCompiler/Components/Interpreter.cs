/*
This class receives an AST an executes the stamentes it represents.
*/

namespace GSharpCompiler;

class Interpreter : IVisitorStmt<object?>, IVisitorExpr<Element>
{
    //Store the state of the program as a nested scope structure
    private Scope globalScope = new Scope();

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
        Interpret(program.Stmts,globalScope);
        return drawables;
    }
    #region Interpret statements
    private object? Interpret(Stmt stmt, Scope scope)
    {
        stmt.Accept(this, scope);
        return null;
    }
    public object? VisitStmtList(Stmt.StmtList stmtList, Scope scope){
        foreach (Stmt stmt in stmtList)
        {
            Interpret(stmt, scope);
        }
        return null;
    }
    public object? VisitEmptyStmt(Stmt.Empty emptyStmt, Scope scope)
    {
        return null;
    }
    public object? VisitPointStmt(Stmt.Point point, Scope scope)
    {
        scope.SetArgument(point.Id.Lexeme, new Element.Point(new Element.String(point.Id.Lexeme), point.X, point.Y, point.Comment,colorStack.Top));
        return null;
    }
      public object? VisitLinesStmt(Stmt.Lines lines, Scope scope)
    {
        scope.SetArgument(lines.Id.Lexeme, new Element.Lines(new Element.String(lines.Id.Lexeme),(Element.Point)Evaluate(lines.P1,scope) , (Element.Point)(Evaluate(lines.P2,scope)),lines.Comment,colorStack.Top));
        return null;
    }
     public object? VisitSegmentStmt(Stmt.Segment segment, Scope scope)
    {
        scope.SetArgument(segment.Id.Lexeme, new Element.Segment(new Element.String(segment.Id.Lexeme),(Element.Point)Evaluate(segment.P1,scope) , (Element.Point)(Evaluate(segment.P2,scope)),segment.Comment,colorStack.Top));
        return null;
    }
     public object? VisitRayStmt(Stmt.Ray ray, Scope scope)
    {
        scope.SetArgument(ray.Id.Lexeme, new Element.Ray(new Element.String(ray.Id.Lexeme),(Element.Point)Evaluate(ray.P1,scope) , (Element.Point)(Evaluate(ray.P2,scope)),ray.Comment,colorStack.Top));
        return null;
    }

    public object? VisitConstantDeclarationStmt(Stmt.Declaration.Constant declaration, Scope scope)
    {
        scope.SetConstant(declaration.Id.Lexeme, Evaluate(declaration.RValue, scope));
        return null;
    }
    public object? VisitFunctionDeclarationStmt(Stmt.Declaration.Function functionStmt, Scope scope){
        scope.SetArgument(functionStmt.Id.Lexeme,Element.Function.MakeFunction(functionStmt));
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print stmt, Scope scope)
    {
        if(stmt._Expr == Expr.EMPTY)outputStream.WriteLine();
        else outputStream.WriteLine(Evaluate(stmt._Expr, scope));
        return null;
    }
    public object? VisitColorStmt(Stmt.Color stmt, Scope scope)
    {
        if (stmt.IsRestore) colorStack.Pop();
        else colorStack.Push(stmt._Color);
        return null;
    }

    public object? VisitDrawStmt(Stmt.Draw stmt, Scope scope)
    {
        IDrawable drawableElement = (IDrawable)Evaluate(stmt._Expr, scope);
        drawables.Add(drawableElement);
        // Esto lleva arreglo futuro
        return null;
    }

    public object? VisitEvalStmt(Stmt.Eval stmt, Scope scope){
        Evaluate(stmt.Expr,scope);
        return null;
    }

    #endregion Interpret statements

    #region Interpret expressions
    //Evaluate expressionss
    public Element Evaluate(Expr expr, Scope scope)
    {
        return expr.Accept(this, scope);
    }
    
    public Element VisitEmptyExpr(Expr.Empty expr,Scope scope){
        return Element.UNDEFINED;
    }

    public Element VisitNumberExpr(Expr.Number expr, Scope scope)
    {
        return expr.Value;
    }

    public Element VisitStringExpr(Expr.String expr, Scope scope)
    {
        return expr.Value;
    }

    public Element VisitVariableExpr(Expr.Variable expr, Scope scope)
    {
        return scope.Get(expr.Id.Lexeme);
    }

    public Element VisitUnaryNotExpr(Expr.Unary.Not unaryNot, Scope scope)
    {
        Element rValue = Evaluate(unaryNot._Expr, scope);
        return IsNotTruty(rValue);
    }

    public Element VisitUnaryMinusExpr(Expr.Unary.Minus unaryMinus, Scope scope){
        Element.Number rValue = (Element.Number) Evaluate(unaryMinus._Expr,scope);
        return -rValue;
    }

    public Element VisitBinaryPowerExpr(Expr.Binary.Power powerExpr, Scope scope){
        Element.Number left = (Element.Number) Evaluate(powerExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(powerExpr.Right,scope);
        return left ^ right;
    }

    public Element VisitBinaryProductExpr(Expr.Binary.Product productExpr, Scope scope){
        Element.Number left = (Element.Number) Evaluate(productExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(productExpr.Right,scope);
        return left * right;
    }

    public Element VisitBinaryDivisionExpr(Expr.Binary.Division divisionExpr, Scope scope){
        Element.Number left = (Element.Number) Evaluate(divisionExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(divisionExpr.Right,scope);
        if(right.Value == 0.0f)throw new RuntimeException(divisionExpr.Line,divisionExpr.Operator.Offset,"Division by 0");
        return left / right;
    }

    public Element VisitBinaryModulusExpr(Expr.Binary.Modulus modulusExpr, Scope scope){
        Element.Number left = (Element.Number) Evaluate(modulusExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(modulusExpr.Right,scope);
        if(right.Value == 0.0f)throw new RuntimeException(modulusExpr.Line,modulusExpr.Operator.Offset,"Division by 0");
        return left % right;
    }

    public Element VisitBinarySumExpr(Expr.Binary.Sum sumExpr, Scope scope){
        Element.Number left = (Element.Number) Evaluate(sumExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(sumExpr.Right,scope);
        return left + right;
    }

    public Element VisitBinaryDifferenceExpr(Expr.Binary.Difference differenceExpr, Scope scope){
        Element.Number left = (Element.Number) Evaluate(differenceExpr.Left,scope);
        Element.Number right = (Element.Number) Evaluate(differenceExpr.Right,scope);
        return left - right;
    }

    public Element VisitBinaryLessExpr(Expr.Binary.Less lessExpr, Scope scope){
        Element.Number left =(Element.Number) Evaluate(lessExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(lessExpr.Right,scope);
        return left < right;
    }

    public Element VisitBinaryLessEqualExpr(Expr.Binary.LessEqual lessEqualExpr, Scope scope){
        Element.Number left =(Element.Number) Evaluate(lessEqualExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(lessEqualExpr.Right,scope);
        return left <= right;
    }

    public Element VisitBinaryGreaterExpr(Expr.Binary.Greater greaterExpr, Scope scope){
        Element.Number left =(Element.Number) Evaluate(greaterExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(greaterExpr.Right,scope);
        return left > right;
    }

    public Element VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual greaterEqualExpr, Scope scope){
        Element.Number left =(Element.Number) Evaluate(greaterEqualExpr.Left,scope);
        Element.Number right =(Element.Number) Evaluate(greaterEqualExpr.Right,scope);
        return left >= right;
    }

    public Element VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual equalEqualExpr, Scope scope){
        Element left = Evaluate(equalEqualExpr.Left,scope);
        Element right = Evaluate(equalEqualExpr.Right,scope);
        return left.EqualTo(right);
    }

    public Element VisitBinaryNotEqualExpr(Expr.Binary.NotEqual notEqualExpr, Scope scope){
        Element left = Evaluate(notEqualExpr.Left,scope);
        Element right = Evaluate(notEqualExpr.Right,scope);
        return left.NotEqualTo(right);
    }

    public Element VisitBinaryAndExpr(Expr.Binary.And andExpr, Scope scope){
        if(IsTruthy(Evaluate(andExpr.Left,scope)) == Element.FALSE)return Element.FALSE;//Shortcircuit
        return IsTruthy(Evaluate(andExpr.Right,scope));
    }

    public Element VisitBinaryOrExpr(Expr.Binary.Or orExpr, Scope scope){
        if(IsTruthy(Evaluate(orExpr.Left,scope)) == Element.TRUE)return Element.TRUE;//Shortcircuit
        return IsTruthy(Evaluate(orExpr.Right,scope));
    }

    public Element VisitConditionalExpr(Expr.Conditional conditionalExpr, Scope scope){
        if( IsTruthy(Evaluate(conditionalExpr.Condition,scope)) == Element.TRUE ) return Evaluate(conditionalExpr.ThenBranchExpr,scope);
        return Evaluate(conditionalExpr.ElseBranchExpr,scope);
    }

    public Element VisitLetInExpr(Expr.LetIn letInExpr, Scope scope){
        Scope letInScope = new Scope(scope);
        foreach(Stmt stmt in letInExpr.LetStmts)Interpret(stmt,letInScope);
        return Evaluate(letInExpr.InExpr,letInScope);
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