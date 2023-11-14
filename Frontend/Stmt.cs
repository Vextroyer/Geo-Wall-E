/*
This class represents statements.
Statements are instruccions that does not produce a value.
*/
namespace Frontend;

public interface Visitor<T>
{
    public T VisitPointStmt(Stmt.Point stmt, Scope scope);
    public T VisitDrawStmt(Stmt.Draw stmt,Scope scope);
}

public abstract class Stmt
{
    abstract public T Accept<T>(Visitor<T> visitor, Scope scope);

    //Represents a `point` statement.
    public class Point : Stmt
    {
        public Token Id { get; private set; }//The identifier
        public string Comment { get; private set; }//A comment associated to the point.

        public float X { get; private set; }
        public float Y { get; private set; }
        public Point(Token _id, float _x, float _y, string _comment)
        {
            Id = _id;
            Comment = _comment;
            X = _x;
            Y = _y;
        }

        public override T Accept<T>(Visitor<T> visitor, Scope scope)
        {
            return visitor.VisitPointStmt(this, scope);
        }
    }

    public class Draw : Stmt
    {
        public Token Id {get; private set;}

        public Draw(Token _id)
        {
            Id= _id;
        }

public override T Accept<T>(Visitor<T> visitor, Scope scope)
        {
            return visitor.VisitDrawStmt(this, scope);
        }
    }
}