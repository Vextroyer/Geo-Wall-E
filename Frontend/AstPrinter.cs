/*
Return an string representation of an Abstract Syntax Tree(AST)
*/
namespace Frontend;

class AstPrinter : IVisitorStmt<string>, IVisitorExpr<string>{
    public string Print(Stmt stmt){
        return stmt.Accept(this,null);
    }

    public string VisitPointStmt(Stmt.Point stmt,Scope scope){
        return $"point({stmt.Id.Lexeme},{stmt.Comment},{stmt.X},{stmt.Y})";
    }

    public string VisitConstantDeclarationStmt(Stmt.ConstantDeclaration stmt,Scope scope){
        return $"{stmt.Id.Lexeme} = {Print(stmt.Rvalue)}";
    }

    public string VisitPrintStmt(Stmt.Print stmt,Scope scope){
        return $"print({Print(stmt._Expr)})";
    }

    public string Print(Expr expr){
        return expr.Accept(this,null);
    }

    public string VisitNumberExpr(Expr.Number expr,Scope scope){
        return expr.Value.ToString();
    }

    public string VisitStringExpr(Expr.String expr,Scope scope){
        return expr.Value.ToString();
    }

    public string VisitVariableExpr(Expr.Variable expr,Scope scope){
        return expr.Id.Lexeme;
    }
}