/*
This class represents statements.
Statements are instruccions that does not produce a value.
*/
namespace Frontend;

public interface Visitor<T>{
    public T VisitPointStmt(Stmt.Point stmt);
}

public abstract class Stmt{
    abstract public T Accept<T>(Visitor<T> visitor);

    //Represents a `point` statement.
    public class Point : Stmt{
        public Token Id {get; private set;}//The identifier
        public string Comment {get; private set;}//A comment associated to the point.

        public Point(Token _id,string _comment = ""){
            Id = _id;
            Comment = _comment;
        }

        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitPointStmt(this);
        }
    }

}