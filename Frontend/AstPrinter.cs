/*
Return an string representation of an Abstract Syntax Tree(AST)
*/
namespace Frontend;

class AstPrinter : IVisitorStmt<string,Element>, IVisitorExpr<string,Element>{
    public string Print(Stmt stmt){
        return stmt.Accept(this,null);
    }

    public string VisitPointStmt(Stmt.Point stmt,Scope<Element> scope){
        return $"point({stmt.Id.Lexeme},{stmt.Comment},{stmt.X},{stmt.Y})";
    }

    public string VisitConstantDeclarationStmt(Stmt.ConstantDeclaration stmt,Scope<Element> scope){
        return $"{stmt.Id.Lexeme} = {Print(stmt.Rvalue)}";
    }

    public string VisitPrintStmt(Stmt.Print stmt,Scope<Element> scope){
        return $"print({Print(stmt._Expr)})";
    }

    public string VisitColorStmt(Stmt.Color stmt,Scope<Element> scope){
        if(stmt.IsRestore)return "restore";
        return $"color {stmt._Color}";
    }
  
    public string VisitDrawStmt(Stmt.Draw stmt, Scope<Element> scope)
    {
        return $"draw({Print(stmt._Expr)})";
    }
  
    public string Print(Expr expr){
        return expr.Accept(this,null);
    }

    public string VisitNumberExpr(Expr.Number expr,Scope<Element> scope){
        return expr.Value.ToString();
    }

    public string VisitStringExpr(Expr.String expr,Scope<Element> scope){
        return expr.Value.ToString();
    }

    public string VisitVariableExpr(Expr.Variable expr,Scope<Element> scope){
        return expr.Id.Lexeme;
    }
    
    public string VisitUnaryNotExpr(Expr.Unary.Not expr, Scope<Element> scope){
        return $"!({Print(expr._Expr)})";
    }

    public string VisitUnaryMinusExpr(Expr.Unary.Minus expr, Scope<Element> scope){
        return $"-({Print(expr._Expr)})";
    }

    public string VisitBinaryPowerExpr(Expr.Binary.Power expr,Scope<Element> scope){
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryProductExpr(Expr.Binary.Product expr,Scope<Element> scope){
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryDivisionExpr(Expr.Binary.Division expr,Scope<Element> scope){
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryModulusExpr(Expr.Binary.Modulus expr,Scope<Element> scope){
        return PrintBinaryExpr(expr);
    }
    public string VisitBinarySumExpr(Expr.Binary.Sum expr,Scope<Element> scope){
        return PrintBinaryExpr(expr);
    }
    public string VisitBinaryDifferenceExpr(Expr.Binary.Difference expr,Scope<Element> scope){
        return PrintBinaryExpr(expr);
    }
    private string PrintBinaryExpr(Expr.Binary expr){
        return $"{expr.Operator.Lexeme}({Print(expr.Left)},{Print(expr.Right)})";
    }
}