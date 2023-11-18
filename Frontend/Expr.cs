/*
Expressions are instructions that represents values.
On this case expressions represent elements.
*/

namespace Frontend;

interface IVisitorExpr<T,U>{
    public T VisitNumberExpr(Expr.Number expr,Scope<U> scope);
    public T VisitStringExpr(Expr.String expr,Scope<U> scope);
    public T VisitVariableExpr(Expr.Variable expr,Scope<U> scope);
    public T VisitUnaryNotExpr(Expr.Unary.Not expr,Scope<U> scope);
    public T VisitUnaryMinusExpr(Expr.Unary.Minus expr,Scope<U> scope);
}
interface IVisitableExpr{
    public T Accept<T,U>(IVisitorExpr<T,U> visitor,Scope<U> scope);
}

//Base class for expressions.
abstract class Expr : IVisitableExpr{
    public int Line {get; private set;}
    public int Offset {get; private set;}
    protected Expr(int _line,int _offset){
        Line = _line;
        Offset = _offset;
    }

    //Required to work in conjuction with the VisitorStmt interface.
    abstract public T Accept<T,U>(IVisitorExpr<T,U> visitor,Scope<U> scope);

    //Represents the empty expression.
    public class Empty : Expr{
        public Empty():base(0,0){}
        public override T Accept<T,U>(IVisitorExpr<T,U> visitor,Scope<U> scope)
        {
            throw new NotImplementedException();
        }
    }
    public static Empty EMPTY = new Empty();//This is the empty expression.
    
    public class Number : Expr{
        public Element.Number Value {get; private set;}
        public Number(int line,int offset,float _value):base(line,offset){
            Value = new Element.Number(_value);
        }
        public override T Accept<T,U>(IVisitorExpr<T,U> visitor,Scope<U> scope)
        {
            return visitor.VisitNumberExpr(this,scope);
        }
    }
    public class String : Expr{
        public Element.String Value {get; private set;}
        public String(int line,int offset,string _value):base(line,offset){
            Value = new Element.String(_value);
        }
        public override T Accept<T,U>(IVisitorExpr<T,U> visitor,Scope<U> scope)
        {
            return visitor.VisitStringExpr(this,scope);
        }
    }
    //A Variable expression stores an identifier who is asscociated with an element.
    public class Variable : Expr{
        public Token Id {get; private set;}
        public Variable(Token _id):base(_id.Line,_id.Offset){
            Id = _id;
        }
        public override T Accept<T,U>(IVisitorExpr<T,U> visitor,Scope<U> scope)
        {
            return visitor.VisitVariableExpr(this,scope);
        }
    }
    //Base class for unary operators.
    public abstract class Unary : Expr
    {
        public Expr _Expr {get; private set;}
        abstract public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope);
        protected Unary(int line,int offset,Expr _expr):base(line,offset)
        {
            _Expr = _expr;
        }
        //Represents `!` operator.
        public class Not : Unary{
            public Not(int line,int offset,Expr _expr):base(line,offset,_expr){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitUnaryNotExpr(this,scope);
            }
        }
        //Represents `-` operator
        public class Minus : Unary
        {
            public Minus(int line,int offset,Expr _expr):base(line,offset,_expr){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitUnaryMinusExpr(this,scope);
            }
        }
    }
}