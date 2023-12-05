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

    public override void OnErrorFound(int line, int offset, string message, bool enforceAbort = false)
    {
        base.OnErrorFound(line, offset, message, enforceAbort);
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
            try{
                scope.SetArgument(pointStmt.Id.Lexeme, Element.POINT);
            }catch(ScopeException e){
                OnErrorFound(pointStmt.Line,pointStmt.Offset,e.Message);
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
            // if (scope.IsConstant(linesStmt.Id.Lexeme)) OnErrorFound(linesStmt.Line, linesStmt.Offset, $"Redeclaration of constant {linesStmt.Id.Lexeme}");//Rule 1
            // if (scope.HasBinding(linesStmt.Id.Lexeme)) OnErrorFound(linesStmt.Line, linesStmt.Offset, $"Line `{linesStmt.Id.Lexeme}` is declared twice on the same scope");//Rule 3
            CheckLineDeclaration(linesStmt,scope);//Check that expressions contained in the line Stmt are points
            try{
                scope.SetArgument(linesStmt.Id.Lexeme, Element.LINES);
            }catch(ScopeException e){
                OnErrorFound(linesStmt.Line,linesStmt.Offset,e.Message);
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
            // if (scope.IsConstant(segmentStmt.Id.Lexeme)) OnErrorFound(segmentStmt.Line, segmentStmt.Offset, $"Redeclaration of constant {segmentStmt.Id.Lexeme}");//Rule 1
            // if (scope.HasBinding(segmentStmt.Id.Lexeme)) OnErrorFound(segmentStmt.Line, segmentStmt.Offset, $"Segment `{segmentStmt.Id.Lexeme}` is declared twice on the same scope");//Rule 3
            CheckLineDeclaration(segmentStmt,scope);//Check that expressions contained in the line Stmt are points
            try{
                scope.SetArgument(segmentStmt.Id.Lexeme, Element.SEGMENT);
            }catch(ScopeException e){
                OnErrorFound(segmentStmt.Line,segmentStmt.Offset,e.Message);
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
            // if (scope.IsConstant(rayStmt.Id.Lexeme)) OnErrorFound(rayStmt.Line, rayStmt.Offset, $"Redeclaration of constant {rayStmt.Id.Lexeme}");//Rule 1
            // if (scope.HasBinding(rayStmt.Id.Lexeme)) OnErrorFound(rayStmt.Line, rayStmt.Offset, $"Ray `{rayStmt.Id.Lexeme}` is declared twice on the same scope");//Rule 3
            CheckLineDeclaration(rayStmt,scope);//Check that expressions contained in the line Stmt are points
            try{
                scope.SetArgument(rayStmt.Id.Lexeme, Element.RAY);
            }catch(ScopeException e){
                OnErrorFound(rayStmt.Line,rayStmt.Offset,e.Message);
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
            try{
                scope.SetArgument(circleStmt.Id.Lexeme, Element.CIRCLE);
            }catch(ScopeException e){
                OnErrorFound(circleStmt.Line,circleStmt.Offset,e.Message);
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
            try{
                scope.SetArgument(arcStmt.Id.Lexeme, Element.ARC);
            }catch(ScopeException e){
                OnErrorFound(arcStmt.Line,arcStmt.Offset,e.Message);
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
            try{
                scope.SetConstant(declStmt.Id.Lexeme, rValue);
            }catch(ScopeException e){
                OnErrorFound(declStmt.Line,declStmt.Offset,e.Message);
            }
        }
        catch (RecoveryModeException)
        {
            //If a redeclaration is detected then the variable remains with it older type and the checking continues.
        }
        return null;
    }
    public object? VisitFunctionDeclarationStmt(Stmt.Declaration.Function functionStmt, Scope scope){
        try{
            //Do not check the body of the function, delegate it to runtime.
            try{
                scope.SetArgument(functionStmt.Id.Lexeme,Element.Function.MakeFunction(functionStmt.Arity));
            }catch(ScopeException e){
                OnErrorFound(functionStmt.Line,functionStmt.Offset,e.Message);
            }
        }catch(RecoveryModeException){
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
            if (!(element is IDrawable)) OnErrorFound(drawStmt._Expr.Line, drawStmt._Expr.Offset, $"Element of type `{element.Type}` is not drawable");
        }
        catch (RecoveryModeException) { }
        return null;
    }
    public object? VisitEvalStmt(Stmt.Eval evalStmt, Scope scope){
        Check(evalStmt.Expr,scope);//Check the semantic of the expression.
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
        try{
            return scope.Get(variableExpr.Id.Lexeme);
        }catch(ScopeException e){
            //Using an undeclared variable cannot be recovered here because the return type cant be determined.
            OnErrorFound(variableExpr.Line, variableExpr.Offset,e.Message);//Rule 4
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
        try
        {
            if (rValue.Type == ElementType.RUNTIME_DEFINED){
                unaryMinusExpr.RequiresRuntimeCheck = true;
                return Element.NUMBER;
            }
            if (rValue.Type != ElementType.NUMBER) OnErrorFound(unaryMinusExpr.Line, unaryMinusExpr.Offset, $"Applied `-` operator to a {rValue.Type} operand");
            unaryMinusExpr.RequiresRuntimeCheck = false;
        }
        catch (RecoveryModeException)
        {
            //A unary minus always returns a number, therefore this error is recoverable.
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
        CheckNumberOperands(productExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryDivisionExpr(Expr.Binary.Division divisionExpr, Scope scope)
    {
        CheckNumberOperands(divisionExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryModulusExpr(Expr.Binary.Modulus modulusExpr, Scope scope)
    {
        CheckNumberOperands(modulusExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinarySumExpr(Expr.Binary.Sum sumExpr, Scope scope)
    {
        CheckNumberOperands(sumExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryDifferenceExpr(Expr.Binary.Difference differenceExpr, Scope scope)
    {
        CheckNumberOperands(differenceExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryLessExpr(Expr.Binary.Less lessExpr, Scope scope)
    {
        CheckNumberOperands(lessExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryLessEqualExpr(Expr.Binary.LessEqual lessEqualExpr, Scope scope)
    {
        CheckNumberOperands(lessEqualExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryGreaterExpr(Expr.Binary.Greater greaterExpr, Scope scope)
    {
        CheckNumberOperands(greaterExpr, scope);
        return Element.NUMBER;
    }
    public Element VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual greaterEqualExpr, Scope scope)
    {
        CheckNumberOperands(greaterEqualExpr, scope);
        return Element.NUMBER;
    }
    private void CheckNumberOperands(Expr.Binary binaryExpr, Scope scope)
    {
        bool runtimeCheck = false;
        try
        {
            Element operand = Check(binaryExpr.Left, scope);//Check left operand
            if(operand.Type == ElementType.RUNTIME_DEFINED){
                runtimeCheck = true;
            }
            else if (operand.Type != ElementType.NUMBER) OnErrorFound(binaryExpr.Left.Line, binaryExpr.Left.Offset, $"Left operand of `{binaryExpr.Operator.Lexeme}` is {operand.Type} and must be NUMBER");
        }
        catch (RecoveryModeException) { }
        try
        {
            Element operand = Check(binaryExpr.Right, scope);//Check right operand
            if(operand.Type == ElementType.RUNTIME_DEFINED){
                runtimeCheck = true;
            }
            else if (operand.Type != ElementType.NUMBER) OnErrorFound(binaryExpr.Right.Line, binaryExpr.Right.Offset, $"Right operand of `{binaryExpr.Operator.Lexeme}` is {operand.Type} and must be NUMBER");
        }
        catch (RecoveryModeException) { }
        //On error recovery mode assume that the operands are numbers and continue, this works because the return type of the methods
        //that use this method is always a number.
        binaryExpr.RequiresRuntimeCheck = runtimeCheck;
    }
    private void CheckLineDeclaration(Stmt.Lines lineStmt, Scope scope){
        //Both expressions must be points in order to build the line
        try{
            Element parameter = Check(lineStmt.P1,scope);
            if(parameter.Type != ElementType.POINT)OnErrorFound(lineStmt.Line,lineStmt.Offset,$"Expected `POINT` as first parameter but {parameter.Type} was found");
        }
        catch(RecoveryModeException){}
        try{
            Element parameter = Check(lineStmt.P2,scope);
            if(parameter.Type != ElementType.POINT)OnErrorFound(lineStmt.Line,lineStmt.Offset,$"Expected `POINT` as second parameter but {parameter.Type} was found");
        }catch(RecoveryModeException){}
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
        if(thenBranchElement.Type == ElementType.RUNTIME_DEFINED) return elseBranchElement;
        if(elseBranchElement.Type == ElementType.RUNTIME_DEFINED) return thenBranchElement;
        
        //This error can't be recovered here, it will be recovered on a lower node of the syntax tree.
        if (thenBranchElement.Type != elseBranchElement.Type) OnErrorFound(conditionalExpr.Line, conditionalExpr.Offset, $"Expected equal return types for `if-then-else` expression branches, but {thenBranchElement.Type} and {elseBranchElement.Type} were found.");

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
    public Element VisitCallExpr(Expr.Call callExpr, Scope scope){
        try{
            //Check that the identifier associated to the call corresponds to a given function.
            scope.Get(callExpr.Id.Lexeme,callExpr.Arity);
            //Check the parameters of the function
            foreach(Expr parameterExpr in callExpr.Parameters){
                try{
                    Check(parameterExpr,scope);
                }
                catch(RecoveryModeException){
                    //If a parameter has semantic issues detect it as an error, but continue checking the other parameters.
                }
            }
        }catch(ScopeException e){
            OnErrorFound(callExpr.Line,callExpr.Offset,e.Message);
        }
        //Cannot recover after the call because the return type of the call cant be determined.
        return Element.RUNTIME_DEFINED;
    }
}