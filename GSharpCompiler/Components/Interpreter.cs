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

    ///<summary>The number of calls on the call stack.</summary>
    private int callStackCounter = 0;
    ///<summary>The limit of calls a program can execute without incurring on a stack overflow.</summary>
    private int callStackSize = 1000;

    public Interpreter(TextWriter _outputStream)
    {
        outputStream = _outputStream;
    }

    //Interpret a program.
    public List<IDrawable> Interpret(Program program)
    {
        Interpret(program.Stmts, globalScope);
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
        scope.SetArgument(point.Id.Lexeme, new Element.Point(new Element.String(point.Id.Lexeme), point.X, point.Y, point.Comment, colorStack.Top));
        return null;
    }
      public object? VisitLinesStmt(Stmt.Lines lines, Scope scope)
    {
        scope.SetArgument(lines.Id.Lexeme, new Element.Lines(new Element.String(lines.Id.Lexeme), (Element.Point)Evaluate(lines.P1, scope), (Element.Point)(Evaluate(lines.P2, scope)), lines.Comment, colorStack.Top));
        return null;
    }
     public object? VisitSegmentStmt(Stmt.Segment segment, Scope scope)
    {
        scope.SetArgument(segment.Id.Lexeme, new Element.Segment(new Element.String(segment.Id.Lexeme), (Element.Point)Evaluate(segment.P1, scope), (Element.Point)(Evaluate(segment.P2, scope)), segment.Comment, colorStack.Top));
        return null;
    }
     public object? VisitRayStmt(Stmt.Ray ray, Scope scope)
    {
        scope.SetArgument(ray.Id.Lexeme, new Element.Ray(new Element.String(ray.Id.Lexeme), (Element.Point)Evaluate(ray.P1, scope), (Element.Point)(Evaluate(ray.P2, scope)), ray.Comment, colorStack.Top));
        return null;
    }
    public object? VisitCircleStmt(Stmt.Circle circle, Scope scope)
    {
        scope.SetArgument(circle.Id.Lexeme, new Element.Circle(new Element.String(circle.Id.Lexeme), (Element.Point)Evaluate(circle.P1, scope), circle.Radius, circle.Comment, colorStack.Top));
        
        return null;
    }
    public object? VisitArcStmt(Stmt.Arc arc, Scope scope)
    {
        scope.SetArgument(arc.Id.Lexeme, new Element.Arc(new Element.String(arc.Id.Lexeme), (Element.Point)Evaluate(arc.P1, scope), (Element.Point)Evaluate(arc.P2, scope), (Element.Point)Evaluate(arc.P3, scope),arc.Radius, arc.Comment, colorStack.Top));
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
        if (stmt._Expr == Expr.EMPTY) outputStream.WriteLine();
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
        Element rValue = Evaluate(unaryMinus._Expr,scope);
        if(unaryMinus.RequiresRuntimeCheck && rValue is not Element.Number)throw new RuntimeException(unaryMinus.Line,unaryMinus.Offset,$"Applied `-` operator to element of type {rValue.Type}");
        return -(rValue as Element.Number)!;
    }

    public Element VisitBinaryPowerExpr(Expr.Binary.Power powerExpr, Scope scope){
        Element left = Evaluate(powerExpr.Left,scope);
        if(powerExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(powerExpr.Line,powerExpr.Offset,$"Left operand of `^` is of type {left.Type}");
        Element right = Evaluate(powerExpr.Right,scope);
        if(powerExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(powerExpr.Line,powerExpr.Offset,$"Right operand of `^` is of type {right.Type}");
        return (left as Element.Number)! ^ (right as Element.Number)!;
    }

    public Element VisitBinaryProductExpr(Expr.Binary.Product productExpr, Scope scope){
        Element left = Evaluate(productExpr.Left,scope);
        if(productExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(productExpr.Line,productExpr.Offset,$"Left operand of `*` is of type {left.Type}");
        Element right = Evaluate(productExpr.Right,scope);
        if(productExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(productExpr.Line,productExpr.Offset,$"Right operand of `*` is of type {right.Type}");
        return (left as Element.Number)! * (right as Element.Number)!;
    }

    public Element VisitBinaryDivisionExpr(Expr.Binary.Division divisionExpr, Scope scope){
        Element left = Evaluate(divisionExpr.Left,scope);
        if(divisionExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(divisionExpr.Line,divisionExpr.Offset,$"Left operand of `/` is of type {left.Type}");
        Element right = Evaluate(divisionExpr.Right,scope);
        if(divisionExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(divisionExpr.Line,divisionExpr.Offset,$"Right operand of `/` is of type {right.Type}");
        if((right as Element.Number)!.Value == 0.0f)throw new RuntimeException(divisionExpr.Line,divisionExpr.Operator.Offset,"Division by 0");
        return (left as Element.Number)! / (right as Element.Number)!;
    }

    public Element VisitBinaryModulusExpr(Expr.Binary.Modulus modulusExpr, Scope scope){
        Element left = Evaluate(modulusExpr.Left,scope);
        if(modulusExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(modulusExpr.Line,modulusExpr.Offset,$"Left operand of `%` is of type {left.Type}");
        Element right = Evaluate(modulusExpr.Right,scope);
        if(modulusExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(modulusExpr.Line,modulusExpr.Offset,$"Right operand of `%` is of type {right.Type}");
        if((right as Element.Number)!.Value == 0.0f)throw new RuntimeException(modulusExpr.Line,modulusExpr.Operator.Offset,"Division by 0");
        return (left as Element.Number)! % (right as Element.Number)!;
    }

    public Element VisitBinarySumExpr(Expr.Binary.Sum sumExpr, Scope scope){
        Element left = Evaluate(sumExpr.Left,scope);
        if(sumExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(sumExpr.Line,sumExpr.Offset,$"Left operand of `+` is of type {left.Type}");
        Element right = Evaluate(sumExpr.Right,scope);
        if(sumExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(sumExpr.Line,sumExpr.Offset,$"Right operand of `+` is of type {right.Type}");
        return (left as Element.Number)! + (right as Element.Number)!;
    }

    public Element VisitBinaryDifferenceExpr(Expr.Binary.Difference differenceExpr, Scope scope){
        Element left = Evaluate(differenceExpr.Left,scope);
        if(differenceExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(differenceExpr.Line,differenceExpr.Offset,$"Left operand of `-` is of type {left.Type}");
        Element right = Evaluate(differenceExpr.Right,scope);
        if(differenceExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(differenceExpr.Line,differenceExpr.Offset,$"Right operand of `-` is of type {right.Type}");
        return (left as Element.Number)! - (right as Element.Number)!;
    }

    public Element VisitBinaryLessExpr(Expr.Binary.Less lessExpr, Scope scope){
        Element left = Evaluate(lessExpr.Left,scope);
        if(lessExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(lessExpr.Line,lessExpr.Offset,$"Left operand of `<` is of type {left.Type}");
        Element right = Evaluate(lessExpr.Right,scope);
        if(lessExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(lessExpr.Line,lessExpr.Offset,$"Right operand of `<` is of type {right.Type}");
        return (left as Element.Number)! < (right as Element.Number)!;
    }

    public Element VisitBinaryLessEqualExpr(Expr.Binary.LessEqual lessEqualExpr, Scope scope){
        Element left = Evaluate(lessEqualExpr.Left,scope);
        if(lessEqualExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(lessEqualExpr.Line,lessEqualExpr.Offset,$"Left operand of `<=` is of type {left.Type}");
        Element right = Evaluate(lessEqualExpr.Right,scope);
        if(lessEqualExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(lessEqualExpr.Line,lessEqualExpr.Offset,$"Right operand of `<=` is of type {right.Type}");
        return (left as Element.Number)! <= (right as Element.Number)!;
    }

    public Element VisitBinaryGreaterExpr(Expr.Binary.Greater greaterExpr, Scope scope){
        Element left = Evaluate(greaterExpr.Left,scope);
        if(greaterExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(greaterExpr.Line,greaterExpr.Offset,$"Left operand of `>` is of type {left.Type}");
        Element right = Evaluate(greaterExpr.Right,scope);
        if(greaterExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(greaterExpr.Line,greaterExpr.Offset,$"Right operand of `>` is of type {right.Type}");
        return (left as Element.Number)! > (right as Element.Number)!;
    }

    public Element VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual greaterEqualExpr, Scope scope){
        Element left = Evaluate(greaterEqualExpr.Left,scope);
        if(greaterEqualExpr.RequiresRuntimeCheck && left is not Element.Number)throw new RuntimeException(greaterEqualExpr.Line,greaterEqualExpr.Offset,$"Left operand of `>=` is of type {left.Type}");
        Element right = Evaluate(greaterEqualExpr.Right,scope);
        if(greaterEqualExpr.RequiresRuntimeCheck && right is not Element.Number)throw new RuntimeException(greaterEqualExpr.Line,greaterEqualExpr.Offset,$"Right operand of `>=` is of type {right.Type}");
        return (left as Element.Number)! >= (right as Element.Number)!;
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
        //This could lead to security holes, because if the conditional is declared inside a function then it would not be checked for having the same return type for both operands.
        if( IsTruthy(Evaluate(conditionalExpr.Condition,scope)) == Element.TRUE ) return Evaluate(conditionalExpr.ThenBranchExpr,scope);
        return Evaluate(conditionalExpr.ElseBranchExpr,scope);
    }

    public Element VisitLetInExpr(Expr.LetIn letInExpr, Scope scope){
        Scope letInScope = new Scope(scope);
        foreach(Stmt stmt in letInExpr.LetStmts)Interpret(stmt,letInScope);
        return Evaluate(letInExpr.InExpr,letInScope);
    }

    public Element VisitCallExpr(Expr.Call callExpr, Scope scope){
        //Retrieve the declaration of the function.
        Element.Function calledFunction = (Element.Function) scope.Get(callExpr.Id.Lexeme,callExpr.Arity);
        //Compute the value of the parameters.
        List<Element> parameters = new List<Element>(callExpr.Arity);
        foreach(Expr expr in callExpr.Parameters)parameters.Add(Evaluate(expr,scope));
        //Create a new scope for the function.
        Scope functionScope = Scope.RequestScopeForFunction(calledFunction.Arguments,parameters,scope);
        //Excecute the body of the function on the new scope.
        ++callStackCounter;
        if(callStackCounter > callStackSize)throw new RuntimeException(callExpr.Line,callExpr.Offset,$"Stack overflow. Last called function was {callExpr.Id.Lexeme}");
        Element result = Evaluate(calledFunction.Body,functionScope);
        --callStackCounter;
        return result;
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
    private Element.Number IsNotTruty(Element element)
    {
        Element.Number truthValue = IsTruthy(element);
        if (truthValue == Element.TRUE) return Element.FALSE;
        return Element.TRUE;
    }
}