/*
Expressions are instructions that represents values.
On this case expressions represent elements.
*/

using System.Collections;

namespace GSharpCompiler;

interface IVisitorExpr<T>
{
    public T VisitEmptyExpr(Expr.Empty expr, Scope scope);
    public T VisitNumberExpr(Expr.Number expr, Scope scope);
    public T VisitStringExpr(Expr.String expr, Scope scope);
    public T VisitVariableExpr(Expr.Variable expr, Scope scope);
    public T VisitUnaryNotExpr(Expr.Unary.Not expr, Scope scope);
    public T VisitUnaryMinusExpr(Expr.Unary.Minus expr, Scope scope);
    public T VisitBinaryPowerExpr(Expr.Binary.Power expr, Scope scope);
    public T VisitBinaryProductExpr(Expr.Binary.Product expr, Scope scope);
    public T VisitBinaryDivisionExpr(Expr.Binary.Division expr, Scope scope);
    public T VisitBinaryModulusExpr(Expr.Binary.Modulus expr, Scope scope);
    public T VisitBinarySumExpr(Expr.Binary.Sum expr, Scope scope);
    public T VisitBinaryDifferenceExpr(Expr.Binary.Difference expr, Scope scope);
    public T VisitBinaryLessExpr(Expr.Binary.Less expr, Scope scope);
    public T VisitBinaryLessEqualExpr(Expr.Binary.LessEqual expr, Scope scope);
    public T VisitBinaryGreaterExpr(Expr.Binary.Greater expr, Scope scope);
    public T VisitBinaryGreaterEqualExpr(Expr.Binary.GreaterEqual expr, Scope scope);
    public T VisitBinaryEqualEqualExpr(Expr.Binary.EqualEqual expr, Scope scope);
    public T VisitBinaryNotEqualExpr(Expr.Binary.NotEqual expr, Scope scope);
    public T VisitBinaryAndExpr(Expr.Binary.And expr, Scope scope);
    public T VisitBinaryOrExpr(Expr.Binary.Or expr, Scope scope);
    public T VisitConditionalExpr(Expr.Conditional expr, Scope scope);
    public T VisitLetInExpr(Expr.LetIn expr, Scope scope);
    public T VisitCallExpr(Expr.Call expr,Scope scope);
    public T VisitMeasureExpr(Expr.Measure expr,Scope scope);
    public T VisitPointExpr(Expr.Point expr, Scope scope);
    public T VisitLinesExpr(Expr.Lines expr, Scope scope);
    public T VisitSegmentExpr(Expr.Segment expr, Scope scope);
    public T VisitRayExpr(Expr.Ray expr, Scope scope);
    public T VisitCircleExpr(Expr.Circle expr, Scope scope);
    public T VisitArcExpr(Expr.Arc expr, Scope scope);
    public T VisitSequenceExpr(Expr.Sequence expr, Scope scope);
    public T VisitCountExpr(Expr.Count expr,Scope scope);
    public T VisitRandomsExpr(Expr.Randoms expr,Scope scope);
    public T VisitSamplesExpr(Expr.Samples expr,Scope scope);
}
interface IVisitableExpr
{
    public T Accept<T>(IVisitorExpr<T> visitor, Scope scope);
}

//Base class for expressions.
abstract class Expr : IVisitableExpr, IErrorLocalizator
{
    public int Line { get; private set; }
    public int Offset { get; private set; }
    public string File { get => new string(fileName); }
    private char[] fileName;
    public char[] ExposeFile { get => fileName; }
    public virtual bool RequiresRuntimeCheck { get; set; }
    protected Expr(int _line, int _offset, char[] _fileName)
    {
        Line = _line;
        Offset = _offset;
        fileName = _fileName;
        RequiresRuntimeCheck = true;
    }

    //Required to work in conjuction with the VisitorStmt interface.
    abstract public T Accept<T>(IVisitorExpr<T> visitor, Scope scope);

    //Represents the empty expression.
    public class Empty : Expr
    {
        public Empty() : base(0, 0, new char[] { 'E', 'M', 'P', 'T', 'Y' }) { }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitEmptyExpr(this, scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    public static Empty EMPTY = new Empty();//This is the empty expression.

    public class Number : Expr
    {
        public Element.Number Value { get; private set; }
        public Number(int line, int offset, char[] fileName, float _value) : base(line, offset, fileName)
        {
            Value = new Element.Number(_value);
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitNumberExpr(this, scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    public class Measure : Expr{
        ///<summary>The expression representing the first point.</summary>
        public Expr P1 {get; private set;}
        ///<summary>The expression representing the second point.</summary>
        public Expr P2 {get; private set;}
        public Measure(Token measureToken,Expr firstPoint, Expr secondPoint):base(measureToken.Line,measureToken.Offset,measureToken.ExposeFile){
            P1 = firstPoint;
            P2 = secondPoint;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitMeasureExpr(this,scope);
        }
    }
    public class String : Expr
    {
        public Element.String Value { get; private set; }
        public String(int line, int offset, char[] fileName, string _value) : base(line, offset, fileName)
        {
            Value = new Element.String(_value);
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitStringExpr(this, scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    //A Variable expression stores an identifier who is asscociated with an element.
    public class Variable : Expr
    {
        public Token Id { get; private set; }
        public Variable(Token _id, char[] fileName) : base(_id.Line, _id.Offset, fileName)
        {
            Id = _id;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitVariableExpr(this, scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    ///<summary>Represent function calls.</summary>
    public class Call : Expr
    {
        public Token Id { get; private set; }
        public List<Expr> Parameters { get; private set; }
        public int Arity { get => Parameters.Count; }
        public Call(Token id, char[] fileName, List<Expr> parameters) : base(id.Line, id.Offset, fileName)
        {
            Id = id;
            Parameters = parameters;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitCallExpr(this, scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    //Base class for unary operators.
    public abstract class Unary : Expr
    {
        public Expr _Expr { get; private set; }
        abstract public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope);
        protected Unary(int line, int offset, char[] fileName, Expr _expr) : base(line, offset, fileName)
        {
            _Expr = _expr;
        }
        //Represents `!` operator.
        public class Not : Unary
        {
            public Not(int line, int offset, char[] fileName, Expr _expr) : base(line, offset, fileName, _expr) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitUnaryNotExpr(this, scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents `-` operator
        public class Minus : Unary
        {
            public Minus(int line, int offset, char[] fileName, Expr _expr) : base(line, offset, fileName, _expr) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitUnaryMinusExpr(this, scope);
            }
        }
    }
    //Base class for binary operators.
    public abstract class Binary : Expr
    {
        ///<summary>Determine the specialization of Expr.Binary to be created for the given operator.For example 
        ///`GetTypeFromOperation(TokenType.PLUS);` returns Expr.Binary.Sum</summary>
        ///<returns>A Type derived from Expr.Binary.</returns>
        private static Type GetTypeFromOperation(TokenType operationType)
        {
            switch (operationType)
            {
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
        public static Expr.Binary MakeBinaryExpr(int line, int offset, char[] fileName, Token operation, Expr left, Expr right)
        {
            Type binaryExprType = GetTypeFromOperation(operation.Type);
            //Create an object derived from Expr.Binary
            Expr.Binary binaryExpr = (Activator.CreateInstance(binaryExprType, line, offset, fileName, operation, left, right) as Expr.Binary)!;
            return binaryExpr;
        }
        public Expr Left { get; private set; }
        public Expr Right { get; private set; }
        public Token Operator { get; private set; }
        abstract public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope);
        protected Binary(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName)
        {
            Left = left;
            Right = right;
            Operator = _operator;
        }
        //Represents the `^` operator for exponentiation.
        public class Power : Binary
        {
            public Power(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryPowerExpr(this, scope);
            }
        }
        //Represents the `*` operator for multiplication.
        public class Product : Binary
        {
            public Product(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryProductExpr(this, scope);
            }
        }
        //Represents the `/` operator for division.
        public class Division : Binary
        {
            public Division(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryDivisionExpr(this, scope);
            }
        }
        //Represents the `%` operator for modulus.
        public class Modulus : Binary
        {
            public Modulus(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryModulusExpr(this, scope);
            }
        }
        //Represents the `+` operator for sum
        public class Sum : Binary
        {
            public Sum(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinarySumExpr(this, scope);
            }
        }
        //Represents the `-` operator for difference
        public class Difference : Binary
        {
            public Difference(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryDifferenceExpr(this, scope);
            }
        }
        //Represents the `<` operator for less-than relation
        public class Less : Binary
        {
            public Less(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryLessExpr(this, scope);
            }
        }
        //Represents the `<=` operator for less-than or equal relation
        public class LessEqual : Binary
        {
            public LessEqual(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryLessEqualExpr(this, scope);
            }
        }
        //Represents the `>` operator for greater-than relation
        public class Greater : Binary
        {
            public Greater(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryGreaterExpr(this, scope);
            }
        }
        //Represents the `>=` operator for greater-than or equal relation
        public class GreaterEqual : Binary
        {
            public GreaterEqual(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryGreaterEqualExpr(this, scope);
            }
        }
        //Represents the `==` operator for greater-than relation
        public class EqualEqual : Binary
        {
            public EqualEqual(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryEqualEqualExpr(this, scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents the `!=` operator for greater-than relation
        public class NotEqual : Binary
        {
            public NotEqual(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryNotEqualExpr(this, scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents the `&` operator for logical and
        public class And : Binary
        {
            public And(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryAndExpr(this, scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
        //Represents the `|` operator for logical or
        public class Or : Binary
        {
            public Or(int line, int offset, char[] fileName, Token _operator, Expr left, Expr right) : base(line, offset, fileName, _operator, left, right) { }
            public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
            {
                return visitor.VisitBinaryOrExpr(this, scope);
            }
            public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
        }
    }
    //Conditional operator if-then-else
    public class Conditional : Expr
    {
        public Expr Condition { get; private set; }
        public Expr ThenBranchExpr { get; private set; }
        public Expr ElseBranchExpr { get; private set; }
        public Conditional(int line, int offset, char[] fileName, Expr condition, Expr thenBranchExpr, Expr elseBranchExpr) : base(line, offset, fileName)
        {
            Condition = condition;
            ThenBranchExpr = thenBranchExpr;
            ElseBranchExpr = elseBranchExpr;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitConditionalExpr(this, scope);
        }
    }
    //Let-in expressions
    public class LetIn : Expr
    {
        ///<summary>The statements after the `let` keyword.</summary>
        public Stmt.StmtList LetStmts { get; private set; }
        ///<summary>The expression after the `in` keyword.</summary>
        public Expr InExpr { get; private set; }
        public LetIn(int line, int offset, char[] fileName, Stmt.StmtList stmts, Expr expr) : base(line, offset, fileName)
        {
            LetStmts = stmts;
            InExpr = expr;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitLetInExpr(this, scope);
        }
        public override bool RequiresRuntimeCheck { get => false; set => base.RequiresRuntimeCheck = false; }
    }
    public class Point : Expr
    {
        public Expr X { get; private set; }
        public Expr Y { get; private set; }
        public bool FullDeclarated { get; private set; }
        public Point(int line, int offset, char[] fileName, Expr x, Expr y) : base(line, offset, fileName)
        {
            X = x;
            Y = y;
            FullDeclarated = true;
        }
        public Point(int line, int offset, char[] fileName) : base(line, offset, fileName)
        {
            FullDeclarated = false;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitPointExpr(this, scope);
        }
    }
    public class Lines : Expr
    {
        public Expr P1 { get; private set; }//first point
        public Expr P2 { get; private set; }//second 
        public bool FullDeclarated { get; private set; }

        public Lines(int line, int offset, char[] fileName, Expr _p1, Expr _p2) : base(line, offset, fileName)
        {
            P1 = _p1;
            P2 = _p2;
            FullDeclarated = true;
        }
        public Lines(int line, int offset, char[] fileName) : base(line, offset, fileName)
        {
            FullDeclarated = false;
            P1 = new Expr.Empty();
            P2 = new Expr.Empty();
        }

        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitLinesExpr(this, scope);
        }
    }
    public class Segment : Expr
    {
        public Expr P1 { get; private set; }//first point
        public Expr P2 { get; private set; }//second 
        public bool FullDeclarated { get; private set; }


        public Segment(int line, int offset, char[] fileName, Expr _p1, Expr _p2) : base(line, offset, fileName)
        {
            P1 = _p1;
            P2 = _p2;
            FullDeclarated = true;

        }
        public Segment(int line, int offset, char[] fileName) : base(line, offset, fileName)
        {
            P1 = new Expr.Empty();
            P2 = new Expr.Empty();
            FullDeclarated = false;

        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitSegmentExpr(this, scope);
        }
    }
    public class Ray : Expr
    {
        public Expr P1 { get; private set; }//first point
        public Expr P2 { get; private set; }//second 
        public bool FullDeclarated { get; private set; }

        public Ray(int line, int offset, char[] fileName, Expr _p1, Expr _p2) : base(line, offset, fileName)
        {
            P1 = _p1;
            P2 = _p2;
            FullDeclarated = true;

        }
        public Ray(int line, int offset, char[] fileName) : base(line, offset, fileName)
        {
            P1 = new Expr.Empty();
            P2 = new Expr.Empty();
            FullDeclarated = false;

        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitRayExpr(this, scope);
        }
    }
    public class Circle : Expr
    {
        public Expr P1 { get; private set; }
        public Expr Radius { get; private set; }//radio  
        public bool FullDeclarated { get; private set; }
        public Circle(int line, int offset, char[] fileName, Expr _p1, Expr radius) : base(line, offset, fileName)
        {
            P1 = _p1;
            Radius = radius;
            FullDeclarated = true;
        }
        public Circle(int line, int offset, char[] fileName) : base(line, offset, fileName)
        {
            FullDeclarated = false;
            P1 = new Expr.Empty();
            Radius= new Expr.Empty();
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitCircleExpr(this, scope);
        }
    }
    public class Arc : Expr
    {
        public Expr P1 { get; private set; }//first point
        public Expr P2 { get; private set; }//first point
        public Expr P3 { get; private set; }//first point
        public Expr Radius { get; private set; }//radio 
        public bool FullDeclarated { get; private set; }
        public Arc(int _line, int _offset, char[] fileName, Expr _p1, Expr _p2, Expr _p3, Expr radius) : base(_line, _offset, fileName)
        {

            P1 = _p1;
            P2 = _p2;
            P3 = _p3;
            Radius = radius;
            FullDeclarated = true;

        }
        public Arc(int _line, int _offset, char[] fileName) : base(_line, _offset, fileName)
        {

            P1 = new Expr.Empty();
            P2 = new Expr.Empty();
            P3 = new Expr.Empty();
            Radius=new Expr.Empty();

            FullDeclarated = false;

        }


        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitArcExpr(this, scope);
        }
    }

    ///<summary>Sequence expressions.</summary>
    public class Sequence : Expr, IEnumerable<Expr>{
        public Expr First {get => Expressions[0];}
        public Expr Second {get => Expressions[1];}
        ///<summary>Does this sequence has three dots.</summary>
        public bool HasTreeDots {get; private set;}
        ///<summary>The elements of the sequence</summary>
        public List<Expr> Expressions {get; private set;}
        ///<summary>Is this sequence empty.</summary>
        public bool IsEmpty {get => Expressions.Count == 0;}
        public int Count => Expressions.Count;
        
        public Sequence(int line,int offset,char[] fileName,bool hasTreeDots,List<Expr> sequence):base(line,offset,fileName){
            HasTreeDots = hasTreeDots;
            Expressions = sequence;
            if(HasTreeDots && (sequence.Count == 0 || sequence.Count > 2))throw new Exception($"Tried to form a dotted sequence with {sequence.Count} elements");
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitSequenceExpr(this,scope);
        }

        IEnumerator<Expr> IEnumerable<Expr>.GetEnumerator()
        {
            return ((IEnumerable<Expr>)Expressions).GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Expressions).GetEnumerator();
        }
    }
    public class Count : Expr{
        public new Expr Sequence {get; private set;}
        public Count(int line,int offset,char[] fileName, Expr sequence):base(line,offset,fileName){
            Sequence = sequence;
        }
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitCountExpr(this,scope);
        }
    }
    public class Randoms : Expr{
        public Randoms(Token randomToken):base(randomToken.Line,randomToken.Offset,randomToken.ExposeFile){}
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitRandomsExpr(this,scope);
        }
    }
    public class Samples : Expr{
        public Samples(Token samplesToken):base(samplesToken.Line,samplesToken.Offset,samplesToken.ExposeFile){}
        public override T Accept<T>(IVisitorExpr<T> visitor, Scope scope)
        {
            return visitor.VisitSamplesExpr(this,scope);
        }
    }
}