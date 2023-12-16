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

    private Element.Sequence.Randoms randomNumbers = new Element.Sequence.Randoms();
    private Element.Sequence.Samples randomPoints = new Element.Sequence.Samples();

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
    public object? VisitStmtList(Stmt.StmtList stmtList, Scope scope)
    {
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
        if (point.FullDeclarated)
        {
            scope.SetArgument(point.Id.Lexeme, new Element.Point(new Element.String(point.Id.Lexeme), (Element.Number)Evaluate(point.X, scope), (Element.Number)Evaluate(point.Y, scope), point.Comment, colorStack.Top));
        }
        else
        {
            scope.SetArgument(point.Id.Lexeme, new Element.Point(colorStack.Top));
        }
        return null;
    }
    public object? VisitLinesStmt(Stmt.Lines lines, Scope scope)
    {
        if (lines.FullDeclarated)
        {
            scope.SetArgument(lines.Id.Lexeme, new Element.Lines(new Element.String(lines.Id.Lexeme), (Element.Point)Evaluate(lines.P1, scope), (Element.Point)(Evaluate(lines.P2, scope)), lines.Comment, colorStack.Top));
        }
        else
        {
            scope.SetArgument(lines.Id.Lexeme, new Element.Lines(colorStack.Top));
        }
        return null;
    }
    public object? VisitSegmentStmt(Stmt.Segment segment, Scope scope)
    {
        if (segment.FullDeclarated)
        {
            scope.SetArgument(segment.Id.Lexeme, new Element.Segment(new Element.String(segment.Id.Lexeme), (Element.Point)Evaluate(segment.P1, scope), (Element.Point)(Evaluate(segment.P2, scope)), segment.Comment, colorStack.Top));
        }
        else { scope.SetArgument(segment.Id.Lexeme, new Element.Segment(colorStack.Top)); }
        return null;
    }
    public object? VisitRayStmt(Stmt.Ray ray, Scope scope)
    {
        if (ray.FullDeclarated)
        {
            scope.SetArgument(ray.Id.Lexeme, new Element.Ray(new Element.String(ray.Id.Lexeme), (Element.Point)Evaluate(ray.P1, scope), (Element.Point)(Evaluate(ray.P2, scope)), ray.Comment, colorStack.Top));
        }
        else { scope.SetArgument(ray.Id.Lexeme, new Element.Ray(colorStack.Top)); }
        return null;
    }
    public object? VisitCircleStmt(Stmt.Circle circle, Scope scope)
    {
        if (circle.FullDeclarated)
        {
            scope.SetArgument(circle.Id.Lexeme, new Element.Circle(new Element.String(circle.Id.Lexeme), (Element.Point)Evaluate(circle.P1, scope), (Element.Measure)Evaluate(circle.Radius, scope), circle.Comment, colorStack.Top));
        }
        else { scope.SetArgument(circle.Id.Lexeme, new Element.Circle(colorStack.Top)); }
        return null;
    }
    public object? VisitArcStmt(Stmt.Arc arc, Scope scope)
    {
        if (arc.FullDeclarated)
        {
            scope.SetArgument(arc.Id.Lexeme, new Element.Arc(new Element.String(arc.Id.Lexeme), (Element.Point)Evaluate(arc.P1, scope), (Element.Point)Evaluate(arc.P2, scope), (Element.Point)Evaluate(arc.P3, scope), (Element.Measure)Evaluate(arc.Radius, scope), arc.Comment, colorStack.Top));
        }
        else { scope.SetArgument(arc.Id.Lexeme, new Element.Arc(colorStack.Top)); }
        return null;
    }

    public object? VisitConstantDeclarationStmt(Stmt.Declaration.Constant declaration, Scope scope)
    {
        scope.SetConstant(declaration.Id.Lexeme, Evaluate(declaration.RValue, scope));
        return null;
    }
    public object? VisitFunctionDeclarationStmt(Stmt.Declaration.Function functionStmt, Scope scope)
    {
        scope.SetArgument(functionStmt.Id.Lexeme, Element.Function.MakeFunction(functionStmt));
        return null;
    }
    public object? VisitMatchStmt(Stmt.Declaration.Match stmt,Scope scope){
        Element evaluated = Evaluate(stmt.Sequence,scope);
        if(evaluated.Type == ElementType.UNDEFINED){
            foreach(Token id in stmt.Identifiers)if(id.Lexeme != "_")scope.SetArgument(id.Lexeme,Element.UNDEFINED);
            return null;
        }
        Element.Sequence sequence = (evaluated as Element.Sequence)!;
        Element.Sequence.SequenceEnumerator enumerator = (Element.Sequence.SequenceEnumerator)sequence.GetEnumerator();
        //For each identifier except the last one, which is the rest.
        for (int i = 0; i < stmt.Identifiers.Count - 1; ++i)
        {
            //Element to be retrieved from the sequence.
            Element sequenceElement;
            if (stmt.Identifiers[i].Lexeme == "_")
            {
                enumerator.MoveNext();
                continue;
            }
            if (enumerator.MoveNext()) sequenceElement = enumerator.Current;
            else sequenceElement = Element.UNDEFINED;
            //After retrieving the element associate it with the Id.
            scope.SetArgument(stmt.Identifiers[i].Lexeme,sequenceElement);
        }
        //Declare the rest.
        if(stmt.Identifiers[stmt.Identifiers.Count - 1].Lexeme != "_") scope.SetArgument(stmt.Identifiers[stmt.Identifiers.Count - 1].Lexeme,enumerator.Resto);
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
        Element evaluated = Evaluate(stmt._Expr, scope);
        if (evaluated.Type == ElementType.SEQUENCE)
        {
            foreach (Element element in (evaluated as Element.Sequence)!)
            {
                if (!(element is IDrawable)) throw new RuntimeException(stmt, $"Cannot draw `{element.Type}`");
                IDrawable temp = (element as IDrawable)!;
                temp.Color=colorStack.Top;
                drawables.Add((element as IDrawable)!);
            }
            return null;
        }
        if (!(evaluated is IDrawable)) throw new RuntimeException(stmt, $"Cannot draw `{evaluated.Type}`");
        IDrawable drawableElement = (IDrawable)evaluated;
        drawableElement.Comment = stmt.Comment;
        drawables.Add(drawableElement);
        //TODO: fix
        return null;
    }

    public object? VisitEvalStmt(Stmt.Eval stmt, Scope scope)
    {
        Evaluate(stmt.Expr, scope);
        return null;
    }

    #endregion Interpret statements

    #region Interpret expressions
    //Evaluate expressionss
    public Element Evaluate(Expr expr, Scope scope)
    {
        return expr.Accept(this, scope);
    }
    public Element VisitEmptyExpr(Expr.Empty expr, Scope scope)
    {
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
        return OperationTable.Operate("!", Evaluate(unaryNot._Expr, scope));
    }


    public Element VisitUnaryMinusExpr(Expr.Unary.Minus expr, Scope scope)
    {
        Element rvalue = Evaluate(expr._Expr, scope);
        try
        {
            return OperationTable.Operate("-", rvalue);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `-` on {rvalue.Type}");
        }
    }

    public Element VisitBinaryPowerExpr(Expr.Binary.Power expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate("^", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `^` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryProductExpr(Expr.Binary.Product expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate("*", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `*` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryDivisionExpr(Expr.Binary.Division expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate("/", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `/` on {left.Type} and {right.Type}");
        }
        catch (DivideByZeroException)
        {
            throw new RuntimeException(expr, "Division by 0");
        }
    }

    public Element VisitBinaryModulusExpr(Expr.Binary.Modulus expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate("%", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `%` on {left.Type} and {right.Type}");
        }
        catch (DivideByZeroException)
        {
            throw new RuntimeException(expr, "Division by 0");
        }
    }

    public Element VisitBinarySumExpr(Expr.Binary.Sum expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            //return OperationTable.Operate("+", left, right);
            return left + right;
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `+` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryDifferenceExpr(Expr.Binary.Difference expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate("-", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `-` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryLessExpr(Expr.Binary.Less expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate("<", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `<` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryLessEqualExpr(Expr.Binary.LessEqual expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate("<=", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `<=` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryGreaterExpr(Expr.Binary.Greater expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate(">", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `>` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual expr, Scope scope)
    {
        Element left = Evaluate(expr.Left, scope);
        Element right = Evaluate(expr.Right, scope);
        try
        {
            return OperationTable.Operate(">=", left, right);
        }
        catch (InvalidOperationException)
        {
            throw new RuntimeException(expr, $"Cant apply operation `>=` on {left.Type} and {right.Type}");
        }
    }

    public Element VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual equalEqualExpr, Scope scope)
    {
        Element left = Evaluate(equalEqualExpr.Left, scope);
        Element right = Evaluate(equalEqualExpr.Right, scope);
        return left.EqualTo(right);
    }

    public Element VisitBinaryNotEqualExpr(Expr.Binary.NotEqual notEqualExpr, Scope scope)
    {
        Element left = Evaluate(notEqualExpr.Left, scope);
        Element right = Evaluate(notEqualExpr.Right, scope);
        return left.NotEqualTo(right);
    }

    public Element VisitBinaryAndExpr(Expr.Binary.And andExpr, Scope scope)
    {
        if (TruthValue(Evaluate(andExpr.Left, scope)) == Element.FALSE) return Element.FALSE;//Shortcircuit
        return TruthValue(Evaluate(andExpr.Right, scope));
    }

    public Element VisitBinaryOrExpr(Expr.Binary.Or orExpr, Scope scope)
    {
        if (TruthValue(Evaluate(orExpr.Left, scope)) == Element.TRUE) return Element.TRUE;//Shortcircuit
        return TruthValue(Evaluate(orExpr.Right, scope));
    }

    public Element VisitConditionalExpr(Expr.Conditional conditionalExpr, Scope scope)
    {
        //This could lead to security holes, because if the conditional is declared inside a function then it would not be checked for having the same return type for both operands.
        if (TruthValue(Evaluate(conditionalExpr.Condition, scope)) == Element.TRUE) return Evaluate(conditionalExpr.ThenBranchExpr, scope);
        return Evaluate(conditionalExpr.ElseBranchExpr, scope);
    }

    public Element VisitLetInExpr(Expr.LetIn letInExpr, Scope scope)
    {
        Scope letInScope = new Scope(scope);
        foreach (Stmt stmt in letInExpr.LetStmts) Interpret(stmt, letInScope);
        return Evaluate(letInExpr.InExpr, letInScope);
    }
    public Element VisitPointExpr(Expr.Point pointExpr, Scope scope)
    {
        try
        {
            if (pointExpr.FullDeclarated)
            {
                return new Element.Point(new Element.String(""), (Element.Number)Evaluate(pointExpr.X, scope), (Element.Number)Evaluate(pointExpr.Y, scope), new Element.String(""), colorStack.Top);
            }
            else
            {
                return new Element.Point(colorStack.Top);
            }
        }
        catch (RuntimeException e)
        {
            throw new RuntimeException(pointExpr, e.Message);
        }
    }
    public Element VisitLinesExpr(Expr.Lines linesExpr, Scope scope)
    {
        try
        {
            if (linesExpr.FullDeclarated)
            {
                return new Element.Lines(new Element.String(""), (Element.Point)Evaluate(linesExpr.P1, scope), (Element.Point)Evaluate(linesExpr.P2, scope), new Element.String(""), colorStack.Top);
            }
            else
            {
                return new Element.Lines(colorStack.Top);
            }
        }
        catch (RuntimeException e)
        {
            throw new RuntimeException(linesExpr, e.Message);
        }

    }
    public Element VisitSegmentExpr(Expr.Segment linesExpr, Scope scope)
    {
        try
        {
            if (linesExpr.FullDeclarated)
            { return new Element.Segment(new Element.String(""), (Element.Point)Evaluate(linesExpr.P1, scope), (Element.Point)Evaluate(linesExpr.P2, scope), new Element.String(""), colorStack.Top); }
            else { return new Element.Segment(colorStack.Top); }
        }
        catch (RuntimeException e)
        {
            throw new RuntimeException(linesExpr, e.Message);
        }
    }
    public Element VisitRayExpr(Expr.Ray linesExpr, Scope scope)
    {
        try
        {
            if (linesExpr.FullDeclarated)
            { return new Element.Ray(new Element.String(""), (Element.Point)Evaluate(linesExpr.P1, scope), (Element.Point)Evaluate(linesExpr.P2, scope), new Element.String(""), colorStack.Top); }
            else
            { return new Element.Ray(colorStack.Top); }
        }
        catch (RuntimeException e)
        {
            throw new RuntimeException(linesExpr, e.Message);
        }
    }
    public Element VisitCircleExpr(Expr.Circle circleExpr, Scope scope)
    {
        try
        {
            if (circleExpr.FullDeclarated)
            {
                return new Element.Circle(new Element.String(""), (Element.Point)Evaluate(circleExpr.P1, scope), (Element.Measure)Evaluate(circleExpr.Radius, scope), new Element.String(""), colorStack.Top);
            }
            else { return new Element.Circle(colorStack.Top); }
        }
        catch (RuntimeException e)
        {
            throw new RuntimeException(circleExpr, e.Message);
        }
    }
    public Element VisitArcExpr(Expr.Arc circleExpr, Scope scope)
    {
        try
        {
            if (circleExpr.FullDeclarated)
            {
                return new Element.Arc(new Element.String(""), (Element.Point)Evaluate(circleExpr.P1, scope), (Element.Point)Evaluate(circleExpr.P2, scope), (Element.Point)Evaluate(circleExpr.P3, scope), (Element.Measure)Evaluate(circleExpr.Radius, scope), new Element.String(""), colorStack.Top);
            }
            else
            { return new Element.Arc(colorStack.Top); }
        }
        catch (RuntimeException e)
        {
            throw new RuntimeException(circleExpr, e.Message);
        }
    }

    public Element VisitCallExpr(Expr.Call callExpr, Scope scope)
    {
        //Retrieve the declaration of the function.
        Element.Function calledFunction;
        try{
            calledFunction = (Element.Function)scope.Get(callExpr.Id.Lexeme, callExpr.Arity);
        }catch(ScopeException e){
            throw new RuntimeException(callExpr,e.Message);
        }
        //Compute the value of the parameters.
        List<Element> parameters = new List<Element>(callExpr.Arity);
        foreach (Expr expr in callExpr.Parameters) parameters.Add(Evaluate(expr, scope));
        //Create a new scope for the function.
        Scope functionScope = Scope.RequestScopeForFunction(calledFunction.Arguments, parameters, scope);
        //Excecute the body of the function on the new scope.
        ++callStackCounter;
        if (callStackCounter > callStackSize) throw new RuntimeException(callExpr, $"Stack overflow. Last called function was {callExpr.Id.Lexeme}");
        Element result = Evaluate(calledFunction.Body, functionScope);
        --callStackCounter;
        return result;
    }
    public Element VisitMeasureExpr(Expr.Measure expr, Scope scope)
    {
        if (expr.RequiresRuntimeCheck)
        {
            Element p1 = Evaluate(expr.P1, scope);
            if (p1.Type != ElementType.POINT) throw new RuntimeException(expr, $"Expected POINT as first parameter but {p1.Type} was found");
            Element p2 = Evaluate(expr.P2, scope);
            if (p2.Type != ElementType.POINT) throw new RuntimeException(expr, $"Expected POINT as second parameter but {p2.Type} was found");
            return Element.Point.Distance((p1 as Element.Point)!, (p2 as Element.Point)!);
        }
        else
        {
            Element.Point p1 = (Evaluate(expr.P1, scope) as Element.Point)!;
            Element.Point p2 = (Evaluate(expr.P2, scope) as Element.Point)!;
            return Element.Point.Distance(p1, p2);
        }
    }
    ///<summary>Build a sequence from a sequence expr.</summary>
    public Element VisitSequenceExpr(Expr.Sequence sequence, Scope scope)
    {
        if (sequence.HasTreeDots)
        {
            Element start = Evaluate(sequence.First, scope);
            if (start.Type != ElementType.NUMBER) throw new RuntimeException(sequence.First, $"Dotted sequece contains {start.Type} but can only contain NUMBER");
            float startValue = (start as Element.Number)!.Value;
            if (sequence.Count == 2)
            {
                Element end = Evaluate(sequence.Second, scope);
                if (end.Type != ElementType.NUMBER) throw new RuntimeException(sequence.Second, $"Dotted sequece contains {end.Type} but can only contain NUMBER");
                float endValue = (end as Element.Number)!.Value;
                if (startValue > endValue) throw new RuntimeException(sequence, $"Sequence range is inverted : [{startValue} , {endValue}]");
                return new Element.Sequence.Interval(startValue, endValue);
            }
            return new Element.Sequence.Interval(startValue);
        }
        ElementType? type = null;
        List<Element> elements = new List<Element>();
        foreach (Expr expr in sequence)
        {
            Element element = Evaluate(expr, scope);
            if (type == null) type = element.Type;
            if (type != element.Type) throw new RuntimeException(sequence, $"Sequence has elements of type {type} and {element.Type}. Only one type is allowed.");
            elements.Add(element);
        }
        return new Element.Sequence.Listing(elements);
    }
    public Element VisitIntersectExpr(Expr.Intersect intersect, Scope scope)
    {
        Element element1 = Evaluate(intersect.FirstExpression, scope);//aqui faltan try y catch probablemente
        Element element2 = Evaluate(intersect.SecondExpression, scope);
        if (Utils.IsDraweable(element1) && Utils.IsDraweable(element2))
        {
            Element elements = Interceptions.Intercept(element1, element2);
            return elements;
        }
        else
        {
            throw new RuntimeException(intersect, $"Only Draweables elements can be intercepted");
        }
    }
    public Element VisitCountExpr(Expr.Count expr, Scope scope)
    {
        Element evaluated = Evaluate(expr.Sequence, scope);
        if (evaluated.Type != ElementType.SEQUENCE) throw new RuntimeException(expr, $"Expected `SEQUENCE` as parameter but {evaluated.Type} was found");
        Element.Sequence sequence;
        if (evaluated is Element.Sequence.Listing)
        {
            sequence = (evaluated as Element.Sequence.Listing)!;
        }
        else
        {
            sequence = (evaluated as Element.Sequence.Interval)!;
        }
        return sequence.Count;
    }
    public Element VisitSamplesExpr(Expr.Samples expr, Scope scope)
    {
        return this.randomPoints;
    }
    public Element VisitRandomsExpr(Expr.Randoms expr, Scope scope)
    {
        return this.randomNumbers;
    }
    #endregion Interpret expressions

    //Determine if the given element is true or false. Undefined and 0 are false, everything else is true.
    private Element.Number TruthValue(Element element)
    {
        switch (element.Type)
        {
            case ElementType.NUMBER:
                if (((Element.Number)element).Value != 0f) return Element.TRUE;
                return Element.FALSE;
            case ElementType.UNDEFINED:
                return Element.FALSE;
            case ElementType.SEQUENCE:
                return TruthValue((element as Element.Sequence)!.Count);
            default:
                return Element.TRUE;
        }
    }
    //Returns the opposite truth value of the given element.
    private Element.Number OppossiteTruthValue(Element element)
    {
        Element.Number truthValue = TruthValue(element);
        if (truthValue == Element.TRUE) return Element.FALSE;
        return Element.TRUE;
    }
}