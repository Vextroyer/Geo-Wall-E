/*
Return an string representation of an Abstract Syntax Tree(AST)
*/
namespace Frontend;

class AstPrinter : Visitor<string>{
    public string Print(Stmt stmt){
        return stmt.Accept(this);
    }

    public string VisitPointStmt(Stmt.Point stmt){
        return $"point({stmt.Id.Lexeme},{stmt.Comment})";
    }
}