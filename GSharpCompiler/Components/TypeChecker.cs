/*
The type checker receives an asbtract syntax tree and checks that it
follows some rules.
The type checker doesnt create new elements, it just uses the constants
provided on the Element class to represent the objects.
*/

namespace GSharpCompiler;

class TypeChecker : GSharpCompilerComponent, IVisitorStmt<object?>, IVisitorExpr<Element>
{
    Scope globalScope = new Scope();

    public TypeChecker(int MaxErrorCount, ICollection<Error> errors) : base(MaxErrorCount, errors) { }

    public override void Abort() { throw new TypeCheckerException(); }

    public override void OnErrorFound(IErrorLocalizator error, string message, bool enforceAbort = false)
    {
        base.OnErrorFound(error, message, enforceAbort);
        throw new RecoveryModeException();
    }

    //Analyze the semantic of the program to see if it is correct.
    public void Check(Program program)
    {
        Check(program.Stmts, globalScope);
    }
    private void Check(Stmt stmt, Scope scope)
    {
        stmt.Accept(this, scope);
    }
    //Checking statements
    public object? VisitStmtList(Stmt.StmtList stmtList, Scope scope)
    {
        foreach (Stmt stmt in stmtList)
        {
            try
            {
                Check(stmt, scope);
            }
            catch (RecoveryModeException) { }//An entire statement has been discarded. This could be caused by a conditional expression, or an undeclared variable.
        }
        return null;
    }
    public object? VisitEmptyStmt(Stmt.Empty emptyStmt, Scope scope) { return null; }
    public object? VisitPointStmt(Stmt.Point pointStmt, Scope scope)
    {

        try
        {
            // if (scope.IsConstant(pointStmt.Id.Lexeme)) OnErrorFound(pointStmt.Line, pointStmt.Offset, $"Redeclaration of constant {pointStmt.Id.Lexeme}");//Rule 1
            // if (scope.HasBinding(pointStmt.Id.Lexeme)) OnErrorFound(pointStmt.Line, pointStmt.Offset, $"Point `{pointStmt.Id.Lexeme}` is declared twice on the same scope");//Rule 3
            try
            {
                scope.SetArgument(pointStmt.Id.Lexeme, Element.POINT);
            }
            catch (ScopeException e)
            {
                OnErrorFound(pointStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }

    public object? VisitLinesStmt(Stmt.Lines linesStmt, Scope scope)
    {

        try
        {
            if (linesStmt.FullDeclarated)
            {
                CheckLineDeclaration(linesStmt, scope);//Check that expressions contained in the line Stmt are points
            }
            try
            {
                scope.SetArgument(linesStmt.Id.Lexeme, Element.LINES);
            }
            catch (ScopeException e)
            {
                OnErrorFound(linesStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }
    public object? VisitSegmentStmt(Stmt.Segment segmentStmt, Scope scope)
    {
        try
        {
            if (segmentStmt.FullDeclarated)
            {
                CheckLineDeclaration(segmentStmt, scope);//Check that expressions contained in the line Stmt are points
            }
            try
            {
                scope.SetArgument(segmentStmt.Id.Lexeme, Element.SEGMENT);
            }
            catch (ScopeException e)
            {
                OnErrorFound(segmentStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }
    public object? VisitRayStmt(Stmt.Ray rayStmt, Scope scope)
    {

        try
        {
            if (rayStmt.FullDeclarated)
            {
                CheckLineDeclaration(rayStmt, scope);//Check that expressions contained in the line Stmt are points
            }
            try
            {
                scope.SetArgument(rayStmt.Id.Lexeme, Element.RAY);
            }
            catch (ScopeException e)
            {
                OnErrorFound(rayStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }
    public object? VisitCircleStmt(Stmt.Circle circleStmt, Scope scope)
    {
        try
        {
            try
            {
                scope.SetArgument(circleStmt.Id.Lexeme, Element.CIRCLE);
            }
            catch (ScopeException e)
            {
                OnErrorFound(circleStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }
    public object? VisitArcStmt(Stmt.Arc arcStmt, Scope scope)
    {
        try
        {
            try
            {
                scope.SetArgument(arcStmt.Id.Lexeme, Element.ARC);
            }
            catch (ScopeException e)
            {
                OnErrorFound(arcStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }
    public object? VisitConstantDeclarationStmt(Stmt.Declaration.Constant declStmt, Scope scope)
    {
        try
        {
            Element rValue = Check(declStmt.RValue, scope);
            try
            {
                scope.SetConstant(declStmt.Id.Lexeme, rValue);
            }
            catch (ScopeException e)
            {
                OnErrorFound(declStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }
    public object? VisitFunctionDeclarationStmt(Stmt.Declaration.Function functionStmt, Scope scope)
    {
        try
        {
            //Do not check the body of the function, delegate it to runtime.
            try
            {
                scope.SetArgument(functionStmt.Id.Lexeme, Element.Function.MakeFunction(functionStmt.Arity));
            }
            catch (ScopeException e)
            {
                OnErrorFound(functionStmt, e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //The execution continues as if the function were never declared.
        }
        return null;
    }
    public object? VisitPrintStmt(Stmt.Print printStmt, Scope scope)
    {
        Check(printStmt._Expr, scope);
        return null;
    }
    public object? VisitColorStmt(Stmt.Color colorStmt, Scope scope)
    {
        //A color statement doesnt invloves any checking.
        return null;
    }
    public object? VisitDrawStmt(Stmt.Draw drawStmt, Scope scope)
    {
        Element element = Check(drawStmt._Expr, scope);
        //element is of class Element, but its an instance of a subclass of Element, some of which implements the interface IDrawable. Rule # 5
        try
        {
            if (!(element is IDrawable)) OnErrorFound(drawStmt, $"Element of type `{element.Type}` is not drawable");
        }
        catch (RecoveryModeException) { }
        return null;
    }
    public object? VisitEvalStmt(Stmt.Eval evalStmt, Scope scope)
    {
        Check(evalStmt.Expr, scope);//Check the semantic of the expression.
        return null;
    }
    public object? VisitMatchStmt(Stmt.Declaration.Match stmt,Scope scope){
            Element sequence = Check(stmt.Sequence,scope);
            try{
                if(sequence.Type != ElementType.SEQUENCE && sequence.Type != ElementType.RUNTIME_DEFINED)OnErrorFound(stmt,$"Expected `sequence` after `=` but {sequence.Type} was found");
            }
            catch(RecoveryModeException){}
            try{
                List<Token> identifiers = stmt.Identifiers;
                for(int i=0;i<stmt.Identifiers.Count;++i){
                    if(identifiers[i].Lexeme == "_")continue;
                    try
                    {
                        scope.SetArgument(identifiers[i].Lexeme,Element.RUNTIME_DEFINED);
                    }
                    catch(ScopeException e){
                        OnErrorFound(stmt,e.Message);
                    }
                }
            }catch(RecoveryModeException){}
        return null;
    }
    //Checking expressions
    private Element Check(Expr expr, Scope scope)
    {
        return expr.Accept(this, scope);
    }
    public Element VisitEmptyExpr(Expr.Empty emptyExpr, Scope scope)
    {
        return Element.UNDEFINED;
    }
    public Element VisitNumberExpr(Expr.Number numberExpr, Scope scope)
    {
        return Element.NUMBER;
    }
    public Element VisitStringExpr(Expr.String stringExpr, Scope scope)
    {
        return Element.STRING;
    }
    public Element VisitVariableExpr(Expr.Variable variableExpr, Scope scope)
    {
        try
        {
            return scope.Get(variableExpr.Id.Lexeme);
        }
        catch (ScopeException e)
        {
            //Using an undeclared variable cannot be recovered here because the return type cant be determined.
            OnErrorFound(variableExpr, e.Message);//Rule 4
        }
        //Unreachable code.
        throw new Exception("Invalid excecution path reached");
    }
    public Element VisitUnaryNotExpr(Expr.Unary.Not unaryNotExpr, Scope scope)
    {
        Check(unaryNotExpr._Expr, scope);//Check the right hand expr.
        return Element.NUMBER;//Rule # 6
    }
    public Element VisitUnaryMinusExpr(Expr.Unary.Minus unaryMinusExpr, Scope scope)
    {
        //Check the right hand expr.
        //Rule # 7
        Element rValue = Check(unaryMinusExpr._Expr, scope);
        if (rValue.Type != ElementType.RUNTIME_DEFINED)
        {
            try
            {
                if( OperationTable.Operate("-",rValue).Type == ElementType.NUMBER )unaryMinusExpr.RequiresRuntimeCheck = false;
            }
            catch (InvalidOperationException e)
            {
                try
                {
                    OnErrorFound(unaryMinusExpr, e.Message);
                }
                catch (RecoveryModeException) { }
            }
        }
        return Element.NUMBER;
    }
    public Element VisitBinaryPowerExpr(Expr.Binary.Power powerExpr, Scope scope)
    {
        CheckNumberOperands(powerExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryProductExpr(Expr.Binary.Product productExpr, Scope scope)
    {
        Element left = Check(productExpr.Left,scope);
        Element right = Check(productExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                switch(OperationTable.Operate("*",left,right).Type){
                    case ElementType.NUMBER:
                        productExpr.RequiresRuntimeCheck = false;
                        return Element.NUMBER;
                    case ElementType.MEASURE:
                        productExpr.RequiresRuntimeCheck = false;
                        return Element.MEASURE;
                    default:
                        throw new Exception("Invalid excecution path reached.");
                }
            }
            catch (InvalidOperationException e)
            {
                OnErrorFound(productExpr, e.Message);
            }
        }
        return Element.RUNTIME_DEFINED;//This could be a number or a measure
    }
    public Element VisitBinaryDivisionExpr(Expr.Binary.Division divisionExpr, Scope scope)
    {
        Element left = Check(divisionExpr.Left,scope);
        Element right = Check(divisionExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                try{
                    switch(OperationTable.Operate("/",left,right).Type){
                        case ElementType.NUMBER:
                            divisionExpr.RequiresRuntimeCheck = false;
                            return Element.NUMBER;
                        default:
                            throw new Exception("Invalid excecution path reached.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    OnErrorFound(divisionExpr, e.Message);
                }
            }
            catch (RecoveryModeException) { }
        }
        return Element.NUMBER;
    }
    public Element VisitBinaryModulusExpr(Expr.Binary.Modulus modulusExpr, Scope scope)
    {
        CheckNumberOperands(modulusExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinarySumExpr(Expr.Binary.Sum sumExpr, Scope scope)
    {
        Element left = Check(sumExpr.Left,scope);
        Element right = Check(sumExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                switch(OperationTable.Operate("+",left,right).Type){
                    case ElementType.NUMBER:
                        sumExpr.RequiresRuntimeCheck = false;
                        return Element.NUMBER;
                    case ElementType.MEASURE:
                        sumExpr.RequiresRuntimeCheck = false;
                        return Element.MEASURE;
                    case ElementType.SEQUENCE:
                        return Element.SEQUENCE;
                    default:
                        throw new Exception("Invalid excecution path reached.");
                }
            }
            catch (InvalidOperationException e)
            {
                OnErrorFound(sumExpr, e.Message);
            }
        }
        return Element.RUNTIME_DEFINED;//This could be a number or a measure
    }
    public Element VisitBinaryDifferenceExpr(Expr.Binary.Difference differenceExpr, Scope scope)
    {
        Element left = Check(differenceExpr.Left,scope);
        Element right = Check(differenceExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                switch(OperationTable.Operate("-",left,right).Type){
                    case ElementType.NUMBER:
                        differenceExpr.RequiresRuntimeCheck = false;
                        return Element.NUMBER;
                    case ElementType.MEASURE:
                        differenceExpr.RequiresRuntimeCheck = false;
                        return Element.MEASURE;
                    default:
                        throw new Exception("Invalid excecution path reached.");
                }
            }
            catch (InvalidOperationException e)
            {
                OnErrorFound(differenceExpr, e.Message);
            }
        }
        return Element.RUNTIME_DEFINED;//This could be a number or a measure
    }
    public Element VisitBinaryLessExpr(Expr.Binary.Less lessExpr, Scope scope)
    {
        Element left = Check(lessExpr.Left,scope);
        Element right = Check(lessExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                try{
                    switch(OperationTable.Operate("<",left,right).Type){
                        case ElementType.NUMBER:
                            lessExpr.RequiresRuntimeCheck = false;
                            return Element.NUMBER;
                        default:
                            throw new Exception("Invalid excecution path reached.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    OnErrorFound(lessExpr, e.Message);
                }
            }
            catch (RecoveryModeException) { }
        }
        return Element.NUMBER;
    }
    public Element VisitBinaryLessEqualExpr(Expr.Binary.LessEqual lessEqualExpr, Scope scope)
    {
        Element left = Check(lessEqualExpr.Left,scope);
        Element right = Check(lessEqualExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                try{
                    switch(OperationTable.Operate("<",left,right).Type){
                        case ElementType.NUMBER:
                            lessEqualExpr.RequiresRuntimeCheck = false;
                            return Element.NUMBER;
                        default:
                            throw new Exception("Invalid excecution path reached.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    OnErrorFound(lessEqualExpr, e.Message);
                }
            }
            catch (RecoveryModeException) { }
        }
        return Element.NUMBER;
    }
    public Element VisitBinaryGreaterExpr(Expr.Binary.Greater greaterExpr, Scope scope)
    {
        Element left = Check(greaterExpr.Left,scope);
        Element right = Check(greaterExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                try{
                    switch(OperationTable.Operate(">",left,right).Type){
                        case ElementType.NUMBER:
                            greaterExpr.RequiresRuntimeCheck = false;
                            return Element.NUMBER;
                        default:
                            throw new Exception("Invalid excecution path reached.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    OnErrorFound(greaterExpr, e.Message);
                }
            }
            catch (RecoveryModeException) { }
        }
        return Element.NUMBER;
    }
    public Element VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual greaterEqualExpr, Scope scope)
    {
        Element left = Check(greaterEqualExpr.Left,scope);
        Element right = Check(greaterEqualExpr.Right,scope);
        if(left.Type != ElementType.RUNTIME_DEFINED && right.Type != ElementType.RUNTIME_DEFINED){
            try{
                try{
                    switch((OperationTable.Operate(">=",left,right)).Type){
                        case ElementType.NUMBER:
                            greaterEqualExpr.RequiresRuntimeCheck = false;
                            return Element.NUMBER;
                        default:
                            throw new Exception("Invalid excecution path reached.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    OnErrorFound(greaterEqualExpr, e.Message);
                }
            }
            catch (RecoveryModeException) { }
        }
        return Element.NUMBER;
    }
    private void CheckNumberOperands(Expr.Binary binaryExpr, Scope scope)
    {
        bool runtimeCheck = false;
        try
        {
            Element operand = Check(binaryExpr.Left, scope);//Check left operand
            if (operand.Type == ElementType.RUNTIME_DEFINED)
            {
                runtimeCheck = true;
            }
            else if (operand.Type != ElementType.NUMBER) OnErrorFound(binaryExpr, $"Left operand of `{binaryExpr.Operator.Lexeme}` is {operand.Type} and must be NUMBER");
        }
        catch (RecoveryModeException) { }
        try
        {
            Element operand = Check(binaryExpr.Right, scope);//Check right operand
            if (operand.Type == ElementType.RUNTIME_DEFINED)
            {
                runtimeCheck = true;
            }
            else if (operand.Type != ElementType.NUMBER) OnErrorFound(binaryExpr, $"Right operand of `{binaryExpr.Operator.Lexeme}` is {operand.Type} and must be NUMBER");
        }
        catch (RecoveryModeException) { }
        //On error recovery mode assume that the operands are numbers and continue, this works because the return type of the methods
        //that use this method is always a number.
        binaryExpr.RequiresRuntimeCheck = runtimeCheck;
    }
    private void CheckLineDeclaration(Stmt.Lines lineStmt, Scope scope)
    {
        //Both expressions must be points in order to build the line
        try
        {
            Element parameter = Check(lineStmt.P1, scope);
            if (parameter.Type != ElementType.POINT) OnErrorFound(lineStmt, $"Expected `POINT` as first parameter but {parameter.Type} was found");
        }
        catch (RecoveryModeException) { }
        try
        {
            Element parameter = Check(lineStmt.P2, scope);
            if (parameter.Type != ElementType.POINT) OnErrorFound(lineStmt, $"Expected `POINT` as second parameter but {parameter.Type} was found");
        }
        catch (RecoveryModeException) { }
        //On error recovery mode assume that the parameters are points and continue, this works because the return type of the methods
        //that use this method a well defined type.
    }
    public Element VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual equalEqualExpr, Scope scope)
    {
        Check(equalEqualExpr.Left, scope);
        Check(equalEqualExpr.Right, scope);
        //Equality doesnt present any semantic problems.
        return Element.NUMBER;
    }
    public Element VisitBinaryNotEqualExpr(Expr.Binary.NotEqual notEqualExpr, Scope scope)
    {
        Check(notEqualExpr.Left, scope);
        Check(notEqualExpr.Right, scope);
        //Equality doesnt present any semantic problems.
        return Element.NUMBER;
    }
    public Element VisitBinaryAndExpr(Expr.Binary.And andExpr, Scope scope)
    {
        Check(andExpr.Left, scope);
        Check(andExpr.Right, scope);
        //Logical and doesnt present any semantic problems.
        return Element.NUMBER;
    }
    public Element VisitBinaryOrExpr(Expr.Binary.Or orExpr, Scope scope)
    {
        Check(orExpr.Left, scope);
        Check(orExpr.Right, scope);
        //Logical or doesnt present any semantic problems.
        return Element.NUMBER;
    }
    public Element VisitConditionalExpr(Expr.Conditional conditionalExpr, Scope scope)
    {
        Check(conditionalExpr.Condition, scope);//Check condition
        Element thenBranchElement = Check(conditionalExpr.ThenBranchExpr, scope);//Check if branch expression
        Element elseBranchElement = Check(conditionalExpr.ElseBranchExpr, scope);//Check else branch expression
        if (thenBranchElement.Type == ElementType.RUNTIME_DEFINED) return elseBranchElement;
        if (elseBranchElement.Type == ElementType.RUNTIME_DEFINED) return thenBranchElement;

        //This error can't be recovered here, it will be recovered on a lower node of the syntax tree.
        if (thenBranchElement.Type != elseBranchElement.Type) OnErrorFound(conditionalExpr, $"Expected equal return types for `if-then-else` expression branches, but {thenBranchElement.Type} and {elseBranchElement.Type} were found.");

        conditionalExpr.RequiresRuntimeCheck = false;
        return thenBranchElement;
    }
    public Element VisitLetInExpr(Expr.LetIn letInExpr, Scope scope)
    {
        Scope letInScope = new Scope(scope);
        foreach (Stmt stmt in letInExpr.LetStmts) Check(stmt, letInScope);
        Element inExprElement = Check(letInExpr.InExpr, letInScope);
        return inExprElement;
    }
    public Element VisitCallExpr(Expr.Call callExpr, Scope scope)
    {
        try
        {
            //Check that the identifier associated to the call corresponds to a given function.
            scope.Get(callExpr.Id.Lexeme, callExpr.Arity);
            //Check the parameters of the function
            foreach (Expr parameterExpr in callExpr.Parameters)
            {
                try
                {
                    Check(parameterExpr, scope);
                }
                catch (RecoveryModeException)
                {
                    //If a parameter has semantic issues detect it as an error, but continue checking the other parameters.
                }
            }
        }
        catch (ScopeException e)
        {
            OnErrorFound(callExpr, e.Message);
        }
        //Cannot recover after the call because the return type of the call cant be determined.
        return Element.RUNTIME_DEFINED;
    }
    public Element VisitMeasureExpr(Expr.Measure expr, Scope scope)
    {
        bool requiresRuntimeCheck = false;
        try
        {
            Element p1 = Check(expr.P1, scope);
            if (p1.Type == ElementType.RUNTIME_DEFINED) requiresRuntimeCheck = true;
            else if (p1.Type != ElementType.POINT) OnErrorFound(expr, $"Expected POINT as first parameter but {p1.Type} was found");
        }
        catch (RecoveryModeException)
        {
            //Recover
        }
        try
        {
            Element p2 = Check(expr.P1, scope);
            if (p2.Type == ElementType.RUNTIME_DEFINED) requiresRuntimeCheck = true;
            else if (p2.Type != ElementType.POINT) OnErrorFound(expr, $"Expected POINT as second parameter but {p2.Type} was found");
        }
        catch (RecoveryModeException)
        {
            //Recover
        }
        expr.RequiresRuntimeCheck = requiresRuntimeCheck;
        return Element.MEASURE;
    }
    public Element VisitPointExpr(Expr.Point pointExpr, Scope scope)
    {
        if (pointExpr.FullDeclarated)
        {
            try
            {
                Check(pointExpr.X, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameterx = Check(pointExpr.X, scope);
            if (parameterx.Type != ElementType.NUMBER) OnErrorFound(pointExpr, $"Expected `NUMBER` as first parameter but {parameterx.Type} was found");
            try
            {
                Check(pointExpr.Y, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parametery = Check(pointExpr.Y, scope);
            if (parametery.Type != ElementType.NUMBER) OnErrorFound(pointExpr, $"Expected `NUMBER` as first parameter but {parametery.Type} was found");
        }
        return Element.POINT;
    }
    public Element VisitLinesExpr(Expr.Lines linesexpr, Scope scope)
    {
        if (linesexpr.FullDeclarated)
        {
    
            try
            {
                Check(linesexpr.P1, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameterx = Check(linesexpr.P1, scope);
            if (parameterx.Type != ElementType.POINT) OnErrorFound(linesexpr.P1, $"Expected `POINT` as first parameter but {parameterx.Type} was found");
            try
            {
                Check(linesexpr.P2, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parametery = Check(linesexpr.P2, scope);
            if (parametery.Type != ElementType.POINT) OnErrorFound(linesexpr.P2, $"Expected `POINT` as first parameter but {parametery.Type} was found");
        }

        return Element.LINES;
    }
    public Element VisitSegmentExpr(Expr.Segment linesexpr, Scope scope)
    {
        if (linesexpr.FullDeclarated)
        {
            try
            {
                Check(linesexpr.P1, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameterx = Check(linesexpr.P1, scope);
            if (parameterx.Type != ElementType.POINT) OnErrorFound(linesexpr.P1, $"Expected `POINT` as first parameter but {parameterx.Type} was found");
            try
            {
                Check(linesexpr.P2, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parametery = Check(linesexpr.P2, scope);
            if (parametery.Type != ElementType.POINT) OnErrorFound(linesexpr.P2, $"Expected `POINT` as first parameter but {parametery.Type} was found");
        }
        return Element.SEGMENT;
    }
    public Element VisitRayExpr(Expr.Ray linesexpr, Scope scope)
    {
        if (linesexpr.FullDeclarated)
        {
            try
            {
                Check(linesexpr.P1, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameterx = Check(linesexpr.P1, scope);
            if (parameterx.Type != ElementType.POINT) OnErrorFound(linesexpr.P1, $"Expected `POINT` as first parameter but {parameterx.Type} was found");
            try
            {
                Check(linesexpr.P2, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parametery = Check(linesexpr.P2, scope);
            if (parametery.Type != ElementType.POINT) OnErrorFound(linesexpr.P2, $"Expected `POINT` as first parameter but {parametery.Type} was found");
        }
        return Element.RAY;
    }
    public Element VisitCircleExpr(Expr.Circle circleexpr, Scope scope)
    {
        if (circleexpr.FullDeclarated)
        {
            try
            {
                Check(circleexpr.P1, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameter1 = Check(circleexpr.P1, scope);
            if (parameter1.Type != ElementType.POINT) OnErrorFound(circleexpr.P1, $"Expected `POINT` as first parameter but {parameter1.Type} was found");
            try
            {
                Check(circleexpr.Radius, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameter2 = Check(circleexpr.Radius, scope);
            if (parameter2.Type != ElementType.MEASURE) OnErrorFound(circleexpr.Radius, $"Expected `MEASURE` as first parameter but {parameter2.Type} was found");

            //if( circleexpr.Radius.Type != ElementType.NUMBER) OnErrorFound(circleexpr.Radius, $"Expected `NUMBER` as first parameter but {circleexpr.Radius.Type} was found");
        }
        return Element.CIRCLE;
    }
    public Element VisitArcExpr(Expr.Arc circleexpr, Scope scope)
    {
        if (circleexpr.FullDeclarated)
        {
            try
            {
                Check(circleexpr.P1, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameter1 = Check(circleexpr.P1, scope);
            if (parameter1.Type != ElementType.POINT) OnErrorFound(circleexpr.P1, $"Expected `POINT` as first parameter but {parameter1.Type} was found");
            try
            {
                Check(circleexpr.P2, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameter2 = Check(circleexpr.P2, scope);
            if (parameter2.Type != ElementType.POINT) OnErrorFound(circleexpr.P2, $"Expected `POINT` as first parameter but {parameter2.Type} was found");
            try
            {
                Check(circleexpr.P3, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameter3 = Check(circleexpr.P3, scope);
            if (parameter3.Type != ElementType.POINT) OnErrorFound(circleexpr.P3, $"Expected `POINT` as first parameter but {parameter3.Type} was found");
            try
            {
                Check(circleexpr.Radius, scope);
            }
            catch (RecoveryModeException)
            { }
            Element parameter4 = Check(circleexpr.Radius, scope);
            if (parameter4.Type != ElementType.MEASURE) OnErrorFound(circleexpr.Radius, $"Expected `MEASURE` as first parameter but {parameter4.Type} was found");
        }
        return Element.CIRCLE;
    }

    public Element VisitSequenceExpr(Expr.Sequence expr, Scope scope){
        return Element.SEQUENCE;
    }
    public Element VisitCountExpr(Expr.Count expr,Scope scope){
        Element sequence = Check(expr.Sequence,scope);
        try{
            if(sequence.Type != ElementType.SEQUENCE && sequence.Type != ElementType.RUNTIME_DEFINED)OnErrorFound(expr,$"Expected `SEQUENCE` as parameter but {sequence.Type} was found");
        }catch(RecoveryModeException){}
        return Element.NUMBER;
    }
    public Element VisitSamplesExpr(Expr.Samples expr,Scope scope){
        return Element.SEQUENCE;
    }
    public Element VisitRandomsExpr(Expr.Randoms expr,Scope scope){
        return Element.SEQUENCE;
    }
}