/*
Expressions are instructions that represents values.
On this case expressions represent elements.
*/

namespace GSharpCompiler;

interface IVisitorExpr<T>{
    public T VisitEmptyExpr(Expr.Empty expr, Scope scope);
    public T VisitNumberExpr(Expr.Number expr,Scope scope);
    public T VisitStringExpr(Expr.String expr,Scope scope);
    public T VisitVariableExpr(Expr.Variable expr,Scope scope);
    public T VisitUnaryNotExpr(Expr.Unary.Not expr,Scope scope);
    public T VisitUnaryMinusExpr(Expr.Unary.Minus expr,Scope scope);
    public T VisitBinaryPowerExpr(Expr.Binary.Power expr,Scope scope);
    public T VisitBinaryProductExpr(Expr.Binary.Product expr,Scope scope);
    public T VisitBinaryDivisionExpr(Expr.Binary.Division expr,Scope scope);
    public T VisitBinaryModulusExpr(Expr.Binary.Modulus expr,Scope scope);
    public T VisitBinarySumExpr(Expr.Binary.Sum expr,Scope scope);
    public T VisitBinaryDifferenceExpr(Expr.Binary.Difference expr,Scope scope);
    public T VisitBinaryLessExpr(Expr.Binary.Less expr,Scope scope);
    public T VisitBinaryLessEqualExpr(Expr.Binary.LessEqual expr,Scope scope);
    public T VisitBinaryGreaterExpr(Expr.Binary.Greater expr,Scope scope);
    public T VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual expr,Scope scope);
    public T VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual expr,Scope scope);
    public T VisitBinaryNotEqualExpr(Expr.Binary.NotEqual expr,Scope scope);
    public T VisitBinaryAndExpr(Expr.Binary.And expr,Scope scope);
    public T VisitBinaryOrExpr(Expr.Binary.Or expr,Scope scope);
    public T VisitConditionalExpr(Expr.Conditional expr,Scope scope);
    public T VisitLetInExpr(Expr.LetIn expr, Scope scope);
    public T VisitCallExpr(Expr.Call expr,Scope scope);
}
interface IVisitableExpr{
    public T Accept<T>(IVisitorExpr<T> visitor,Scope scope);
}

//Base class for expressions.
abstract class Expr : IVisitableExpr{
    public int Line {get; private set;}
    public int Offset {get; private set;}
    public virtual bool RequiresRuntimeCheck {get; set;}
    protected Expr(int _line,int _offset){
        Line = _line;
        Offset = _offset;
        RequiresRuntimeCheck = true;
    }

    //Required to work in conjuction with the VisitorStmt interface.
    abstract public T Accept<T>(IVisitorExpr<T> visitor,Scope scope);

    //Represents the empty expression.
    public class Empty : Expr{
        public Empty():base(0,0){}
        public override T Accept<T>(IVisitorExpr<T> visitor,Scope scope)
        {
            return visitor.VisitEmptyExpr(this,scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    public static Empty EMPTY = new Empty();//This is the empty expression.
    
    public class Number : Expr{
        public Element.Number Value {get; private set;}
        public Number(int line,int offset,float _value):base(line,offset){
            Value = new Element.Number(_value);
        }
        public override T Accept<T>(IVisitorExpr<T> visitor,Scope scope)
        {
            return visitor.VisitNumberExpr(this,scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    public class String : Expr{
        public Element.String Value {get; private set;}
        public String(int line,int offset,string _value):base(line,offset){
            Value = new Element.String(_value);
        }
        public override T Accept<T>(IVisitorExpr<T> visitor,Scope scope)
        {
            return visitor.VisitStringExpr(this,scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    //A Variable expression stores an identifier who is asscociated with an element.
    public class Variable : Expr{
        public Token Id {get; private set;}
        public Variable(Token _id):base(_id.Line,_id.Offset){
            Id = _id;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor,Scope scope)
        {
            return visitor.VisitVariableExpr(this,scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    ///<summary>Represent function calls.</summary>
    public class Call : Expr{
        public Token Id {get; private set;}
        public List<Expr> Parameters {get; private set;}
        public int Arity {get => Parameters.Count;}
        public Call(Token id,List<Expr> parameters):base(id.Line,id.Offset){
            Id = id;
            Parameters = parameters;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitCallExpr(this,scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    //Base class for unary operators.
    public abstract class Unary : Expr
    {
        public Expr _Expr {get; private set;}
        abstract public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope);
        protected Unary(int line,int offset,Expr _expr):base(line,offset)
        {
            _Expr = _expr;
        }
        //Represents `!` operator.
        public class Not : Unary{
            public Not(int line,int offset,Expr _expr):base(line,offset,_expr){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitUnaryNotExpr(this,scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents `-` operator
        public class Minus : Unary
        {
            public Minus(int line,int offset,Expr _expr):base(line,offset,_expr){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitUnaryMinusExpr(this,scope);
            }
        }
    }
    //Base class for binary operators.
    public abstract class Binary : Expr{
        ///<summary>Determine the specialization of Expr.Binary to be created for the given operator.For example 
        ///`GetTypeFromOperation(TokenType.PLUS);` returns Expr.Binary.Sum</summary>
        ///<returns>A Type derived from Expr.Binary.</returns>
        private static Type GetTypeFromOperation(TokenType operationType){
            switch(operationType){
                case TokenType.CARET:
                    return typeof(Expr.Binary.Power);
                case TokenType.STAR:
                    return typeof(Expr.Binary.Product);
                case TokenType.SLASH:
                    return typeof(Expr.Binary.Division);
                case TokenType.PERCENT:
                    return typeof(Expr.Binary.Modulus);
                case TokenType.PLUS:
                    return typeof(Expr.Binary.Sum);
                case TokenType.MINUS:
                    return typeof(Expr.Binary.Difference);
                case TokenType.LESS:
                    return typeof(Expr.Binary.Less);
                case TokenType.LESS_EQUAL:
                    return typeof(Expr.Binary.LessEqual);
                case TokenType.GREATER:
                    return typeof(Expr.Binary.Greater);
                case TokenType.GREATER_EQUAL:
                    return typeof(Expr.Binary.GreaterEqual);
                case TokenType.EQUAL_EQUAL:
                    return typeof(Expr.Binary.EqualEqual);
                case TokenType.BANG_EQUAL:
                    return typeof(Expr.Binary.NotEqual);
                case TokenType.AND:
                    return typeof(Expr.Binary.And);
                case TokenType.OR:
                    return typeof(Expr.Binary.Or);
                default: throw new ArgumentException($"No type matching operation of type {operationType}");
            }
        }
        ///<summary>Build a binary expression according to the given operation.</summary>
        public static Expr.Binary MakeBinaryExpr(int line,int offset,Token operation,Expr left,Expr right){
            Type binaryExprType = GetTypeFromOperation(operation.Type);
            //Create an object derived from Expr.Binary
            Expr.Binary binaryExpr = (Activator.CreateInstance(binaryExprType,line,offset,operation,left,right) as Expr.Binary)!;
            return binaryExpr;
        }
        public Expr Left {get; private set;}
        public Expr Right {get; private set;}
        public Token Operator {get; private set;}
        abstract public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope);
        protected Binary(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset){
            Left = left;
            Right = right;
            Operator = _operator;
        }
        //Represents the `^` operator for exponentiation.
        public class Power : Binary{
            public Power(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryPowerExpr(this,scope);
            }
        }
        //Represents the `*` operator for multiplication.
        public class Product : Binary{
            public Product(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryProductExpr(this,scope);
            }
        }
        //Represents the `/` operator for division.
        public class Division : Binary{
            public Division(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryDivisionExpr(this,scope);
            }
        }
        //Represents the `%` operator for modulus.
        public class Modulus : Binary{
            public Modulus(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryModulusExpr(this,scope);
            }
        }
        //Represents the `+` operator for sum
        public class Sum : Binary{
            public Sum(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinarySumExpr(this,scope);
            }
        }
        //Represents the `-` operator for difference
        public class Difference : Binary{
            public Difference(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryDifferenceExpr(this,scope);
            }
        }
        //Represents the `<` operator for less-than relation
        public class Less : Binary{
            public Less(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryLessExpr(this,scope);
            }
        }
        //Represents the `<=` operator for less-than or equal relation
        public class LessEqual : Binary{
            public LessEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryLessEqualExpr(this,scope);
            }
        }
        //Represents the `>` operator for greater-than relation
        public class Greater : Binary{
            public Greater(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryGreaterExpr(this,scope);
            }
        }
        //Represents the `>=` operator for greater-than or equal relation
        public class GreaterEqual : Binary{
            public GreaterEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryGreaterEqualExpr(this,scope);
            }
        }
        //Represents the `==` operator for greater-than relation
        public class EqualEqual : Binary{
            public EqualEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryEqualEqualExpr(this,scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents the `!=` operator for greater-than relation
        public class NotEqual : Binary{
            public NotEqual(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryNotEqualExpr(this,scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents the `&` operator for logical and
        public class And : Binary{
            public And(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryAndExpr(this,scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents the `|` operator for logical or
        public class Or : Binary{
            public Or(int line,int offset,Token _operator,Expr left,Expr right):base(line,offset,_operator,left,right){}
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryOrExpr(this,scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
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
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
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
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitLetInExpr(this,scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
}