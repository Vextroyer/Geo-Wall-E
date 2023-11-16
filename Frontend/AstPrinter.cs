/*
Return an string representation of an Abstract Syntax Tree(AST)
*/
namespace Frontend;

class AstPrinter : Visitor<string>{
    public string Print(Stmt stmt){
        return stmt.Accept(this,null);
    }

    public string VisitPointStmt(Stmt.Point stmt,Scope scope){
        return $"point({stmt.Id.Lexeme},{stmt.Comment},{stmt.X},{stmt.Y})";
    }

    public string VisitDrawStmt(Stmt.Draw stmt, Scope scope)
    {
        return $"draw({stmt.Id})";
    }
}