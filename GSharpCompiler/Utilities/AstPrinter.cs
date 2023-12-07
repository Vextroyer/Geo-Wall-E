/*
Return an string representation of an Abstract Syntax Tree(AST)
*/
namespace GSharpCompiler;

class AstPrinter : IVisitorStmt<string>, IVisitorExpr<string>
{
    public string Print(Stmt stmt)
    {
        return stmt.Accept(this, new Scope());
    }

    public string VisitStmtList(Stmt.StmtList stmtList, Scope scope)
    {
        string result = "";
        foreach (Stmt stmt in stmtList)
        {
            result += Print(stmt) + "\n";
        }
        return result;
    }

    public string VisitEmptyStmt(Stmt.Empty stmt, Scope scope)
    {
        return "EMPTY";
    }

    public string VisitPointStmt(Stmt.Point stmt, Scope scope)
    {
        return $"point({stmt.Id.Lexeme},{stmt.Comment},{stmt.X},{stmt.Y})";
    }
    public string VisitPointExpr(Expr.Point expr, Scope scope)
    {
        return $"point({expr.X},{expr.Y})";
    }
    public string VisitLinesStmt(Stmt.Lines stmt, Scope scope)
    {
        //return $"line({stmt.Id.Lexeme},{stmt.Comment},{VisitVariableExpr((Expr.Variable)stmt.P1,scope)},{VisitVariableExpr((Expr.Variable)stmt.P2,scope)})";
        return $"line({stmt.Id.Lexeme},{stmt.Comment},{Print(stmt.P1)},{Print(stmt.P2)})";
    }
     public string VisitLinesExpr(Expr.Lines expr, Scope scope)
    {
        return $"line({Print(expr.P1)},{Print(expr.P2)}";
    }
    public string VisitRayStmt(Stmt.Ray stmt, Scope scope)
    {
        return $"ray({stmt.Id.Lexeme},{stmt.Comment},{Print(stmt.P1)},{Print(stmt.P2)})";
    }
    public string VisitRayExpr(Expr.Ray stmt, Scope scope)
    {
        return $"ray({Print(stmt.P1)},{Print(stmt.P2)})";
    }
    public string VisitSegmentStmt(Stmt.Segment stmt, Scope scope)
    {
        return $"segment({stmt.Id.Lexeme},{stmt.Comment},{Print(stmt.P1)},{Print(stmt.P2)})";
    }
    public string VisitSegmentExpr(Expr.Segment stmt, Scope scope)
    {
        return $"({Print(stmt.P1)},{Print(stmt.P2)})";
    }
    public string VisitCircleStmt(Stmt.Circle stmt, Scope scope)
    {
        return $"circle({stmt.Id.Lexeme},{stmt.Comment},{Print(stmt.P1)},{stmt.Radius})";
    }
    public string VisitCircleExpr(Expr.Circle stmt, Scope scope)
    {
        return $"({Print(stmt.P1)},{stmt.Radius})";
    }
    public string VisitArcStmt(Stmt.Arc stmt, Scope scope)
    {
        return $"arc({stmt.Id.Lexeme},{stmt.Comment},{Print(stmt.P1)},{Print(stmt.P2)},{Print(stmt.P3)},{stmt.Radius})";
    }
    public string VisitArcExpr(Expr.Arc stmt, Scope scope)
    {
        return $"arc({Print(stmt.P1)},{Print(stmt.P2)},{Print(stmt.P3)},{stmt.Radius})";
    }
    public string VisitConstantDeclarationStmt(Stmt.Declaration.Constant stmt, Scope scope)
    {
        return $"{stmt.Id.Lexeme} = {Print(stmt.RValue)}";
    }

    public string VisitFunctionDeclarationStmt(Stmt.Declaration.Function stmt, Scope scope){
        string ret = $"function {stmt.Id.Lexeme} (";
        for(int i=0;i<stmt.Arguments.Count;++i){
            ret += stmt.Arguments[i].Lexeme;
            if(i < stmt.Arguments.Count - 1)ret += ", ";
        }
        return $"{ret}) = {Print(stmt.Body)}";
    }

    public string VisitEvalStmt(Stmt.Eval stmt, Scope scope){
        return $"eval {Print(stmt.Expr)}";
    }

    public string VisitPrintStmt(Stmt.Print stmt, Scope scope)
    {
        return $"print({Print(stmt._Expr)})";
    }

    public string VisitColorStmt(Stmt.Color stmt, Scope scope)
    {
        if (stmt.IsRestore) return "restore";
        return $"color {stmt._Color}";
    }

    public string VisitDrawStmt(Stmt.Draw stmt, Scope scope)
    {
        return $"draw({Print(stmt._Expr)})";
    }

    public string Print(Expr expr)
    {
        return expr.Accept(this, new Scope());
    }

    public string VisitEmptyExpr(Expr.Empty expr, Scope scope)
    {
        return "EMPTY";
    }

    public string VisitNumberExpr(Expr.Number expr, Scope scope)
    {
        return expr.Value.ToString();
    }

    public string VisitStringExpr(Expr.String expr, Scope scope)
    {
        return expr.Value.ToString();
    }

    public string VisitVariableExpr(Expr.Variable expr, Scope scope)
    {
        return expr.Id.Lexeme;
    }

    public string VisitUnaryNotExpr(Expr.Unary.Not expr, Scope scope)
    {
        return $"!({Print(expr._Expr)})";
    }

    public string VisitUnaryMinusExpr(Expr.Unary.Minus expr, Scope scope)
    {
        return $"-({Print(expr._Expr)})";
    }

    public string VisitBinaryPowerExpr(Expr.Binary.Power expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryProductExpr(Expr.Binary.Product expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryDivisionExpr(Expr.Binary.Division expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryModulusExpr(Expr.Binary.Modulus expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }
    public string VisitBinarySumExpr(Expr.Binary.Sum expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryDifferenceExpr(Expr.Binary.Difference expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryLessExpr(Expr.Binary.Less expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryLessEqualExpr(Expr.Binary.LessEqual expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryGreaterExpr(Expr.Binary.Greater expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryNotEqualExpr(Expr.Binary.NotEqual expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryAndExpr(Expr.Binary.And expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    public string VisitBinaryOrExpr(Expr.Binary.Or expr, Scope scope)
    {
        return PrintBinaryExpr(expr);
    }

    private string PrintBinaryExpr(Expr.Binary expr)
    {
        return $"{expr.Operator.Lexeme}({Print(expr.Left)},{Print(expr.Right)})";
    }

    public string VisitConditionalExpr(Expr.Conditional expr, Scope scope)
    {
        return $"if {Print(expr.Condition)}\nthen {Print(expr.ThenBranchExpr)}\nelse {Print(expr.ElseBranchExpr)}";
    }

    public string VisitLetInExpr(Expr.LetIn expr, Scope scope)
    {
        return $"let({Print(expr.LetStmts)})\nin({Print(expr.InExpr)})";
    }

    public string VisitCallExpr(Expr.Call expr,Scope scope){
        return $"{expr.Id.Lexeme}({PrintArguments()})";

        string PrintArguments(){
            string ret = "";
            for(int i=0;i<expr.Parameters.Count;++i){
                ret += Print(expr.Parameters[i]);
                if(i < expr.Parameters.Count - 1)ret += ',';
            }
            return ret;
        }
    }
    public string VisitMeasureExpr(Expr.Measure expr,Scope scope){
        return $"measure({Print(expr.P1)},{Print(expr.P2)})";
    }
}