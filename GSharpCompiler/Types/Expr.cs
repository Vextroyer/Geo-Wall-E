/*
Expressions are instructions that represents values.
On this case expressions represent elements.
*/

namespace GSharpCompiler;

interface IVisitorExpr<T,U>{
    public T VisitEmptyExpr(Expr.Empty expr, Scope<U> scope);
    public T VisitNumberExpr(Expr.Number expr,Scope<U> scope);
    public T VisitStringExpr(Expr.String expr,Scope<U> scope);
    public T VisitVariableExpr(Expr.Variable expr,Scope<U> scope);
    public T VisitUnaryNotExpr(Expr.Unary.Not expr,Scope<U> scope);
    public T VisitUnaryMinusExpr(Expr.Unary.Minus expr,Scope<U> scope);
    public T VisitBinaryPowerExpr(Expr.Binary.Power expr,Scope<U> scope);
    public T VisitBinaryProductExpr(Expr.Binary.Product expr,Scope<U> scope);
    public T VisitBinaryDivisionExpr(Expr.Binary.Division expr,Scope<U> scope);
    public T VisitBinaryModulusExpr(Expr.Binary.Modulus expr,Scope<U> scope);
    public T VisitBinarySumExpr(Expr.Binary.Sum expr,Scope<U> scope);
    public T VisitBinaryDifferenceExpr(Expr.Binary.Difference expr,Scope<U> scope);
    public T VisitBinaryLessExpr(Expr.Binary.Less expr,Scope<U> scope);
    public T VisitBinaryLessEqualExpr(Expr.Binary.LessEqual expr,Scope<U> scope);
    public T VisitBinaryGreaterExpr(Expr.Binary.Greater expr,Scope<U> scope);
    public T VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual expr,Scope<U> scope);
    public T VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual expr,Scope<U> scope);
    public T VisitBinaryNotEqualExpr(Expr.Binary.NotEqual expr,Scope<U> scope);
    public T VisitBinaryAndExpr(Expr.Binary.And expr,Scope<U> scope);
    public T VisitBinaryOrExpr(Expr.Binary.Or expr,Scope<U> scope);
    public T VisitConditionalExpr(Expr.Conditional expr,Scope<U> scope);
    public T VisitLetInExpr(Expr.LetIn expr, Scope<U> scope);
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
            return visitor.VisitEmptyExpr(this,scope);
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
    //Base class for binary operators.
    public abstract class Binary : Expr{
        public Expr Left {get; private set;}
        public Expr Right {get; private set;}
        public Token Operator {get; private set;}
        abstract public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope);
        protected Binary(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset){
            Left = left;
            Right = right;
            Operator = _operator;
        }
        //Represents the `^` operator for exponentiation.
        public class Power : Binary{
            public Power(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryPowerExpr(this,scope);
            }
        }
        //Represents the `*` operator for multiplication.
        public class Product : Binary{
            public Product(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryProductExpr(this,scope);
            }
        }
        //Represents the `/` operator for division.
        public class Division : Binary{
            public Division(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryDivisionExpr(this,scope);
            }
        }
        //Represents the `%` operator for modulus.
        public class Modulus : Binary{
            public Modulus(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryModulusExpr(this,scope);
            }
        }
        //Represents the `+` operator for sum
        public class Sum : Binary{
            public Sum(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinarySumExpr(this,scope);
            }
        }
        //Represents the `-` operator for difference
        public class Difference : Binary{
            public Difference(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryDifferenceExpr(this,scope);
            }
        }
        //Represents the `<` operator for less-than relation
        public class Less : Binary{
            public Less(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryLessExpr(this,scope);
            }
        }
        //Represents the `<=` operator for less-than or equal relation
        public class LessEqual : Binary{
            public LessEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryLessEqualExpr(this,scope);
            }
        }
        //Represents the `>` operator for greater-than relation
        public class Greater : Binary{
            public Greater(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryGreaterExpr(this,scope);
            }
        }
        //Represents the `>=` operator for greater-than or equal relation
        public class GreaterEqual : Binary{
            public GreaterEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryGreaterEqualExpr(this,scope);
            }
        }
        //Represents the `==` operator for greater-than relation
        public class EqualEqual : Binary{
            public EqualEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryEqualEqualExpr(this,scope);
            }
        }
        //Represents the `!=` operator for greater-than relation
        public class NotEqual : Binary{
            public NotEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryNotEqualExpr(this,scope);
            }
        }
        //Represents the `&` operator for logical and
        public class And : Binary{
            public And(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryAndExpr(this,scope);
            }
        }
        //Represents the `|` operator for logical or
        public class Or : Binary{
            public Or(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
            {
                return visitor.VisitBinaryOrExpr(this,scope);
            }
        }
    }
    //Conditional operator if-then-else
    public class Conditional : Expr
    {
        public Expr Condition {get; private set;}
        public Expr ThenBranchExpr {get; private set;}
        public Expr ElseBranchExpr {get; private set;}
        public Conditional(int line,int offset,Expr condition,Expr thenBranchExpr,Expr elseBranchExpr):base(line,offset){
            Condition = condition;
            ThenBranchExpr = thenBranchExpr;
            ElseBranchExpr = elseBranchExpr;
        }
        public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
        {
            return visitor.VisitConditionalExpr(this,scope);
        }
    }
    //Let-in expressions
    public class LetIn : Expr{
        ///<summary>The statements after the `let` keyword.</summary>
        public Stmt.StmtList LetStmts {get; private set;}
        ///<summary>The expression after the `in` keyword.</summary>
        public Expr InExpr {get; private set;}
        public LetIn(int line,int offset,Stmt.StmtList stmts,Expr expr):base(line,offset){
            LetStmts = stmts;
            InExpr = expr;
        }
        public override T Accept<T, U>(IVisitorExpr<T, U> visitor, Scope<U> scope)
        {
            return visitor.VisitLetInExpr(this,scope);
        }
    }
}