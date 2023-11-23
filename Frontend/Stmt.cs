/*
Statements are instructions that modify the state of the program.
*/
namespace Frontend;


interface IVisitorStmt<T, U>
{
    public T VisitPointStmt(Stmt.Point stmt, Scope<U> scope);
    public T VisitConstantDeclarationStmt(Stmt.ConstantDeclaration stmt, Scope<U> scope);
    public T VisitPrintStmt(Stmt.Print stmt, Scope<U> scope);
    public T VisitColorStmt(Stmt.Color stmt, Scope<U> scope);
    public T VisitDrawStmt(Stmt.Draw stmt, Scope<U> scope);

}
interface IVisitableStmt
{
    public T Accept<T, U>(IVisitorStmt<T, U> visitor, Scope<U> scope);
}
//Base class for statements.
abstract class Stmt : IVisitableStmt
{
    //For error printing
    public int Line { get; private set; }
    public int Offset { get; private set; }
    protected Stmt(int _line, int _offset)
    {
        Line = _line;
        Offset = _offset;
    }
    //Required to work in conjuction with the IVisitorStmt interface.
    abstract public T Accept<T, U>(IVisitorStmt<T, U> visitor, Scope<U> scope);
    //Represents a `point` statement.
    public class Point : Stmt
    {
        public Token Id { get; private set; }//The name of the point will be used as identifier.
        public Element.String Comment { get; private set; }//A comment associated to the point.

        public Element.Number X { get; private set; }//X coordinate
        public Element.Number Y { get; private set; }//Y coordinate
        public Point(int _line, int _offset, Token _id, Element.Number _x, Element.Number _y, Element.String _comment) : base(_line, _offset)
        {
            Id = _id;
            X = _x;
            Y = _y;
            Comment = _comment;
        }

        public override T Accept<T, U>(IVisitorStmt<T, U> visitor, Scope<U> scope)
        {
            return visitor.VisitPointStmt(this, scope);
        }
    }
    //Represents declaration of constants. A constant is a variable whose value cant be modified.
    public class ConstantDeclaration : Stmt
    {
        public Token Id { get; private set; }
        public Expr Rvalue { get; private set; }

        public ConstantDeclaration(Token _id, Expr _expr) : base(_id.Line, _id.Offset)
        {
            Id = _id;
            Rvalue = _expr;
        }

        public override T Accept<T, U>(IVisitorStmt<T, U> visitor, Scope<U> scope)
        {
            return visitor.VisitConstantDeclarationStmt(this, scope);
        }
    }

    public class Draw : Stmt
    {
        public Expr _Expr { get; private set; }

        public Draw(int line,int offset, Expr _expr):base(line,offset)
        {
            _Expr = _expr;
        }

        public override T Accept<T, U>(IVisitorStmt<T, U> visitor, Scope<U> scope)
        {
            return visitor.VisitDrawStmt(this, scope);
        }
    }

    //Print statement
    public class Print : Stmt
    {
        public Expr _Expr { get; private set; }//The expression to be printed.
        public Print(int _line, int _offset, Expr _expr) : base(_line, _offset)
        {
            _Expr = _expr;
        }
        public override T Accept<T, U>(IVisitorStmt<T, U> visitor, Scope<U> scope)
        {
            return visitor.VisitPrintStmt(this, scope);
        }
    }
    //Color statement
    public class Color : Stmt
    {
        public Frontend.Color _Color { get; private set; }
        public bool IsRestore { get; private set; }
        public Color(int line, int offset, Frontend.Color _color, bool _restore = false) : base(line, offset)
        {
            _Color = _color;
            IsRestore = _restore;
        }

        public override T Accept<T, U>(IVisitorStmt<T, U> visitor, Scope<U> scope)
        {
            return visitor.VisitColorStmt(this, scope);
        }
    }
}
