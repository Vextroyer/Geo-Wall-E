/*
Statements are instructions that modify the state of the program.
*/
namespace GSharpCompiler;
using System.Collections;

interface IVisitorStmt<T>
{
    public T VisitEmptyStmt(Stmt.Empty stmt, Scope scope);
    public T VisitPointStmt(Stmt.Point stmt, Scope scope);
    public T VisitConstantDeclarationStmt(Stmt.Declaration.Constant stmt, Scope scope);
    public T VisitFunctionDeclarationStmt(Stmt.Declaration.Function stmt, Scope scope);
    public T VisitPrintStmt(Stmt.Print stmt, Scope scope);
    public T VisitColorStmt(Stmt.Color stmt, Scope scope);
    public T VisitDrawStmt(Stmt.Draw stmt, Scope scope);
    public T VisitStmtList(Stmt.StmtList stmt, Scope scope);
    public T VisitLinesStmt(Stmt.Lines stmt, Scope scope);
    public T VisitSegmentStmt(Stmt.Segment stmt, Scope scope);
    public T VisitRayStmt(Stmt.Ray stmt, Scope scope);
    public T VisitEvalStmt(Stmt.Eval stmt, Scope scope);
    public T VisitCircleStmt(Stmt.Circle stmt, Scope scope);
    public T VisitArcStmt(Stmt.Arc stmt, Scope scope);
}
interface IVisitableStmt
{
    public T Accept<T>(IVisitorStmt<T> visitor, Scope scope);
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
    abstract public T Accept<T>(IVisitorStmt<T> visitor, Scope scope);
    ///<summary>Represents an empty statement.</summary>
    public class Empty : Stmt
    {
        public Empty() : base(0, 0) { }
        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitEmptyStmt(this, scope);
        }
    }
    ///<summary>A constant representing the empty statement. Should be used instead of creating new empty statements.</summary>
    static public Stmt.Empty EMPTY = new Stmt.Empty();

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

        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitPointStmt(this, scope);
        }
    }

    //Represents a `Lines` statement.
    public class Lines : Stmt
    {
        public Token Id { get; protected set; }//The name of the point will be used as identifier.
        public Element.String Comment { get; protected set; }//A comment associated to the line.

        public Expr P1 { get; private set; }//first point
        public Expr P2 { get; private set; }//second 
        public Lines(int _line, int _offset, Token _id, Expr _p1, Expr _p2, Element.String _comment) : base(_line, _offset)
        {
            Id = _id;
            P1 = _p1;
            P2 = _p2;
            Comment = _comment;
        }

        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitLinesStmt(this, scope);
        }
    }
    //Represents a `Segment` statement.
    public class Segment : Lines
    {
        public Segment(int _line, int _offset, Token _id, Expr _p1, Expr _p2, Element.String _comment):base(_line,_offset,_id,_p1,_p2,_comment)
        {}
        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitSegmentStmt(this, scope);
        }
    }
    //Represents a `Ray` statement.
    public class Ray : Lines
    {
        public Ray(int _line, int _offset, Token _id, Expr _p1, Expr _p2, Element.String _comment) : base(_line, _offset,_id,_p1,_p2,_comment)
        {}

        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitRayStmt(this, scope);
        }
    }
     public class Circle : Stmt
    {
        public Token Id { get; private set; }//The name of the point will be used as identifier.
        public Element.String Comment { get; private set; }//A comment associated to the line.

        public Expr P1 { get; private set; }//first point
        public Element.Number Radius { get; private set; }//radio 
        public Circle(int _line, int _offset, Token _id, Expr _p1, Element.Number radius, Element.String _comment) : base(_line, _offset)
        {
            Id = _id;
            P1 = _p1;
            Radius = radius;
            Comment = _comment;
        }

        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitCircleStmt(this, scope);
        }
    }
    public class Arc : Stmt
    {
        public Token Id { get; private set; }//The name of the point will be used as identifier.
        public Element.String Comment { get; private set; }//A comment associated to the line.

        public Expr P1 { get; private set; }//first point
        public Expr P2 { get; private set; }//first point
        public Expr P3 { get; private set; }//first point
        public Element.Number Radius { get; private set; }//radio 
        public Arc(int _line, int _offset, Token _id, Expr _p1,Expr _p2,Expr _p3, Element.Number radius, Element.String _comment) : base(_line, _offset)
        {
            Id = _id;
            P1 = _p1;
            P2 = _p2;
            P3 = _p3;
            Radius = radius;
            Comment = _comment;
        }

        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitArcStmt(this, scope);
        }
    }
    ///<summary>Base class for declarations. A declaration associates an identifier with an Element, for example Number or Function.</summary>
    public abstract class Declaration : Stmt{
        ///<summary>The Identifier which will be binded to the Element.</summary>
        public Token Id {get; protected set;}
        protected Declaration(Token identifier):base(identifier.Line,identifier.Offset){
            Id = identifier;
        }

        abstract public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope);

        ///<summary>Represents declaration of constants.</summary>
        public class Constant : Declaration{
            ///<summary>Expression to be associated to the identifier.</summary>
            public Expr RValue {get; private set;}
            public Constant(Token identifier, Expr rvalue):base(identifier){
                RValue = rvalue;
            }
            public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
            {
                return visitor.VisitConstantDeclarationStmt(this, scope);
            }
        }
        ///<summary>Represents function declarations.</summary>
        public class Function : Declaration{
            ///<summary>The identifiers that made the arguments of the function.</summary>
            public List<Token> Arguments {get; private set;}
            ///<summary>The expr that made the body of the function.</summary>
            public Expr Body {get; private set;}
            public Function(Token identifier,List<Token> arguments, Expr body):base(identifier){
                Arguments = arguments;
                Body = body;
            }
            public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
            {
                return visitor.VisitFunctionDeclarationStmt(this, scope);
            }
            public int Arity {get => Arguments.Count;}
        }
    }

    public class Draw : Stmt
    {
        public Expr _Expr { get; private set; }

        public Draw(int line, int offset, Expr _expr) : base(line, offset)
        {
            _Expr = _expr;
        }

        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitDrawStmt(this, scope);
        }
    }
    ///<summary>An Eval statement allow evaluating an expression as a top level statement.</summary>
    public class Eval : Stmt{
        ///<summary>The expression to be evaluated.</summary>
        public Expr Expr{get; private set;}
        public Eval(int line,int offset,Expr expr):base(line,offset){
            Expr = expr;
        }
        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitEvalStmt(this, scope);
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
        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitPrintStmt(this, scope);
        }
    }
    //Color statement
    public class Color : Stmt
    {
        public GSharpCompiler.Color _Color { get; private set; }
        public bool IsRestore { get; private set; }
        public Color(int line, int offset, GSharpCompiler.Color _color, bool _restore = false) : base(line, offset)
        {
            _Color = _color;
            IsRestore = _restore;
        }

        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitColorStmt(this, scope);
        }
    }

    ///<summary>Represents a list of statements. Its a special kind of statement.</summary>
    public class StmtList : Stmt, ICollection<Stmt>
    {
        ///<summary>The statements that made the statement list.</summary>
        private List<Stmt> stmts;
        public StmtList(int line, int offset) : base(line, offset)
        {
            this.stmts = new List<Stmt>();
        }
        public override T Accept<T>(IVisitorStmt<T> visitor, Scope scope)
        {
            return visitor.VisitStmtList(this, scope);
        }
        ///<summary>Provide access to the subyacent list enumerator.</summary>
        public IEnumerator<Stmt> GetEnumerator()
        {
            return stmts.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        ///<summary>Get the number of statements contained on the list.</summary>
        public int Count { get => stmts.Count; }
        ///<summary>Add the supplied statement to the list.</summary>
        public void Add(Stmt stmt)
        {
            stmts.Add(stmt);
        }
        ///<summary>Removes all statements from the list.</summary>
        public void Clear()
        {
            stmts.Clear();
        }
        ///<summary>Determines wheter a statement is in the list.</summary>
        ///<returns><c>True</c> if <c>stmt</c> is on the list, otherwise false.</returns>
        public bool Contains(Stmt stmt)
        {
            return stmts.Contains(stmt);
        }
        public bool Remove(Stmt stmt)
        {
            return stmts.Remove(stmt);
        }
        public bool IsReadOnly { get => false; }
        public void CopyTo(Stmt[] stmt, int arrayIndex)
        {
            stmts.CopyTo(stmt, arrayIndex);
        }
    }
}
