/*
On this class are defined the different objects that exist on a G# program execution.
*/
using System.Collections;

namespace GSharpCompiler;

//Represents G# objects that can be drawed on screen. Not every object can be drawed.
public interface IDrawable
{
    public ElementType Type { get; }
    public Color Color { get; }
}

public enum ElementType{
    UNDEFINED,
    NUMBER,
    MEASURE,
    STRING,
    POINT,
    LINE,
    SEGMENT,
    RAY,
    CIRCLE,
    ARC,
    FUNCTION_LIST,
    FUNCTION,
    RUNTIME_DEFINED,
    SEQUENCE
}

//Base class for all elements that exist during runtime.
public abstract class Element
{
    //The type of the Element.
    public ElementType Type { get; private set; }

    #region constants
    //Constant elements of the different types
    public static Element.Number NUMBER = new Element.Number(1);
    public static Element.String STRING = new Element.String("");
    public static Element.Point POINT = new Element.Point(STRING, NUMBER, NUMBER, STRING, Color.BLACK);
    public static Element.Lines LINES = new Element.Lines(STRING, POINT, POINT, STRING, Color.BLACK);
    public static Element.Segment SEGMENT = new Element.Segment(STRING, POINT, POINT, STRING, Color.BLACK);
    public static Element.Ray RAY = new Element.Ray(STRING, POINT, POINT, STRING, Color.BLACK);
    public static Element.Circle CIRCLE = new Element.Circle(STRING, POINT, NUMBER, STRING, Color.BLACK);
    public static Element.Arc ARC = new Element.Arc(STRING, POINT,POINT,POINT, NUMBER, STRING, Color.BLACK);
    public static Element.Measure MEASURE = new Element.Measure(1);
    ///<summary>Represents the undefined type. Use this instead of declaring new Undefined objects.</summary>
    public static Element.Undefined UNDEFINED = new Element.Undefined();
    //Boolean values are represented with numbers
    public static Element.Number TRUE = new Element.Number(1);
    public static Element.Number FALSE = new Element.Number(0);
    ///<summary>Represents the runtime_defined type. Use this instead of declaring new RuntimeDefined objects.</summary>
    public static Element.RuntimeDefined RUNTIME_DEFINED = new Element.RuntimeDefined();
    #endregion constants

        protected Element(ElementType type)
    {
        Type = type;
    }
    //Equality operator
    public abstract Element.Number EqualTo(Element other);
    public Element.Number NotEqualTo(Element other)
    {
        if (this.EqualTo(other) == Element.TRUE) return Element.FALSE;
        return Element.TRUE;
    }
    ///<summary>Elements whose type deduction is defered to runtime.</summary>
    public class RuntimeDefined : Element{
        public RuntimeDefined():base(ElementType.RUNTIME_DEFINED){}
        public override Number EqualTo(Element other){throw new InvalidOperationException($"Operation not defined for type {this.Type}");}
    }
    ///<summary>Represents the undefined type.</summary>
    public class Undefined : Element
    {
        public Undefined() : base(ElementType.UNDEFINED) { }
        public override Number EqualTo(Element other)
        {
            if (other.Type == this.Type) return TRUE;
            return FALSE;
        }
    }

    //Represents a real number.
    public class Number : Element
    {
        float value;
        public float Value
        {
            get
            {
                return value;
            }
        }
        public Number(float _value) : base(ElementType.NUMBER)
        {
            value = _value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if(Utils.Compare(this.Value,(other as Element.Number)!.Value) == 0)return Element.TRUE;
            return Element.FALSE;
        }
    }
    ///<summary>Represents measures.</summary>
    public class Measure : Element{
        public float Value {get; private set;}
        public Measure(float value):base(ElementType.MEASURE){
            //Sanitize the input. A measure is always non negative.
            Value = Math.Abs(value);
        }
        public override Number EqualTo(Element other)
        {
            if(other.Type != this.Type)return Element.FALSE;
            if(Utils.Compare(this.Value,(other as Element.Measure)!.Value) == 0)return Element.TRUE;
            return Element.FALSE;
        }
        public override string ToString()
        {
            return Value.ToString() + 'u';
        }
    }
    //Represents a string.
    public class String : Element
    {
        string value;
        public string Value
        {
            get
            {
                return value;
            }
        }
        public String(string _value = "") : base(ElementType.STRING)
        {
            value = _value;
        }
        public override string ToString()
        {
            return value;
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if (((Element.String)other).value == this.value) return Element.TRUE;
            return Element.FALSE;
        }
    }
    ///<summary>Represents a family of functions.</summary>
    internal class FunctionList : Element, IList<Element.Function>
    {
        public List<Element.Function> functions;
        public FunctionList() : base(ElementType.FUNCTION_LIST)
        {
            functions = new List<Element.Function>();
        }
        ///<summary>Determines wheter a function in the list has the given arity.</summary>
        public bool ContainsArity(int arity)
        {
            if (arity < 0) throw new ArgumentException("Arity must be non-negative");
            foreach (Element.Function function in functions) if (function.Arity == arity) return true;
            return false;
        }
        public Function GetFunction(int arity)
        {
            if (arity < 0) throw new ArgumentException("Arity must be non-negative");
            foreach (Element.Function function in functions) if (function.Arity == arity) return function;
            throw new InvalidOperationException("A function with the provided arity was not found");
        }
        public Function this[int index] { get => functions[index]; set => functions[index] = value; }

        public int Count => functions.Count;

        public bool IsReadOnly => ((ICollection<Function>)functions).IsReadOnly;

        public void Add(Function item)
        {
            functions.Add(item);
        }

        public void Clear()
        {
            functions.Clear();
        }

        public bool Contains(Function item)
        {
            return functions.Contains(item);
        }

        public void CopyTo(Function[] array, int arrayIndex)
        {
            functions.CopyTo(array, arrayIndex);
        }

        public override Number EqualTo(Element other)
        {
            throw new NotImplementedException("Cannot test equality on FunctionList");
        }

        public IEnumerator<Function> GetEnumerator()
        {
            return functions.GetEnumerator();
        }

        public int IndexOf(Function item)
        {
            return functions.IndexOf(item);
        }

        public void Insert(int index, Function item)
        {
            functions.Insert(index, item);
        }

        public bool Remove(Function item)
        {
            return functions.Remove(item);
        }

        public void RemoveAt(int index)
        {
            functions.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return functions.GetEnumerator();
        }
    }
    ///<summary>Represents a function.</summary>
    internal class Function : Element
    {
        ///<summary>The arguments of the function.</summary>
        public List<Element.String> Arguments { get; private set; }
        ///<summary>The body of the function.</summary>
        public Expr Body { get; private set; }
        public Function(List<Token> arguments, Expr body) : base(ElementType.FUNCTION)
        {
            Body = body;
            Arguments = new List<Element.String>(arguments.Count);
            foreach (Token identifier in arguments) Arguments.Add(new Element.String(identifier.Lexeme));
        }
        public override Number EqualTo(Element other)
        {
            throw new NotImplementedException("Cannot test equality on Function");
        }
        ///<summary>The arity of the function.</summary>
        public int Arity { get => Arguments.Count; }
        ///<summary>Create a body-less function with the given arity.</summary>
        public static Element.Function MakeFunction(int arity)
        {
            if (arity < 0) throw new ArgumentException("Arity must be a non-negative integer");
            List<Token> arguments = new List<Token>(arity);
            for (int i = 0; i < arity; ++i) arguments.Add(DUMMY);
            return new Element.Function(arguments, Expr.EMPTY);
        }
        ///<summary>Create a function corresponding to the given function declaration.</summary>
        public static Element.Function MakeFunction(Stmt.Declaration.Function funcDecl)
        {
            return new Element.Function(funcDecl.Arguments, funcDecl.Body);
        }
        ///<summary>A dummy token for creating functions that wont be used.</summary>
        private static Token DUMMY = new Token(TokenType.EOF, "", null, -1, -1, new char[] { 'D', 'U', 'M', 'M', 'Y' });
    }
    //Represents a point on a 2D rectangular coordinate system.
    public class Point : Element, IDrawable
    {
        public Element.String name;
        public Element.String comment;
        public Element.Number x;
        public Element.Number y;

        public Color Color { get; private set; }

        public Point(Element.String _name, Element.Number _x, Element.Number _y, Element.String _comment, Color color) : base(ElementType.POINT)
        {
            name = _name;
            comment = _comment;
            x = _x;
            y = _y;
            Color = color;
        }
        public Point(Color color) : base(ElementType.POINT)
        {
            name = new String();
            comment = new String();
            x = new Element.Number(Utils.RandomCoordinate());
            y = new Element.Number(Utils.RandomCoordinate());
            Color = color;

        }
        public Point() : base(ElementType.POINT)
        {
            name = new String();
            comment = new String();
            x = new Element.Number(Utils.RandomCoordinate());
            y = new Element.Number(Utils.RandomCoordinate());
            Color = Color.BLACK;

        }

        public override string ToString()
        {
            return $"{name}({x},{y}){comment}";
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if (((Element.Point)other).x == this.x && ((Element.Point)other).y == this.y) return Element.TRUE;
            return Element.FALSE;
        }
        public static Element.Measure Distance(Element.Point p1,Element.Point p2){
            float xSquare = (OperationTable.Operate("-",p1.x,p2.x) as Element.Number)!.Value;
            xSquare *= xSquare;
            float ySquare = (OperationTable.Operate("-",p1.y,p2.y) as Element.Number)!.Value;
            ySquare *= ySquare;
            return new Element.Measure(float.Sqrt(xSquare + ySquare));
        }
    }
    public class Lines : Element, IDrawable
    {
        public Element.String name;
        public Element.String comment;
        public Element.Point p1;
        public Element.Point p2;

        public Color Color { get; private set; }

        public Lines(Element.String _name, Element.Point _p1, Element.Point _p2, Element.String _comment, Color color) : base(ElementType.LINE)
        {
            name = _name;
            comment = _comment;
            p1 = _p1;
            p2 = _p2;
            Color = color;
        }
        public Lines(Color color) : base(ElementType.LINE)
        {
            name = new String();
            comment = new String();
            p1 = new Element.Point();
            p2 = new Element.Point();
            Color = color;
        }

        public override string ToString()
        {
            return $"{name}(({p1.x},{p1.y}),({p2.x},{p2.y})){comment}";
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if (((Element.Lines)other).p1 == this.p1 && ((Element.Lines)other).p2 == this.p2) return Element.TRUE;
            return Element.FALSE;
        }
    }
    public class Segment : Element, IDrawable
    {
        public Element.String name;
        public Element.String comment;
        public Element.Point p1;
        public Element.Point p2;

        public Color Color { get; private set; }

        public Segment(Element.String _name, Element.Point _p1, Element.Point _p2, Element.String _comment, Color color) : base(ElementType.SEGMENT)
        {
            name = _name;
            comment = _comment;
            p1 = _p1;
            p2 = _p2;
            Color = color;
        }
        public Segment(Color color) : base(ElementType.SEGMENT)
        {
            name = new String();
            comment = new String();
            p1 = new Element.Point();
            p2 = new Element.Point();
            Color = color;
        }

        public override string ToString()
        {
            return $"{name}(({p1.x},{p1.y}),({p2.x},{p2.y})){comment}";
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if (((Element.Segment)other).p1 == this.p1 && ((Element.Segment)other).p2 == this.p2) return Element.TRUE;
            return Element.FALSE;
        }
    }
    public class Ray : Element, IDrawable
    {
        public Element.String name;
        public Element.String comment;
        public Element.Point p1;
        public Element.Point p2;

        public Color Color { get; private set; }

        public Ray(Element.String _name, Element.Point _p1, Element.Point _p2, Element.String _comment, Color color) : base(ElementType.RAY)
        {
            name = _name;
            comment = _comment;
            p1 = _p1;
            p2 = _p2;
            Color = color;
        }
        public Ray(Color color) : base(ElementType.RAY)
        {
            name = new String();
            comment = new String();
            p1 = new Element.Point();
            p2 = new Element.Point();
            Color = color;

        }

        public override string ToString()
        {
            return $"{name}(({p1.x},{p1.y}),({p2.x},{p2.y})){comment}";
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if (((Element.Ray)other).p1 == this.p1 && ((Element.Ray)other).p2 == this.p2) return Element.TRUE;
            return Element.FALSE;
        }
    }
    public class Circle : Element, IDrawable
    {
        public Element.String name;
        public Element.String comment;
        public Element.Point p1;
        public Element.Number radius;

        public Color Color { get; private set; }

        public Circle(Element.String _name, Element.Point _p1, Element.Number _radius, Element.String _comment, Color color) : base(ElementType.CIRCLE)
        {
            name = _name;
            comment = _comment;
            p1 = _p1;
            radius = _radius;
            Color = color;
        }
        public Circle(Color color) : base(ElementType.CIRCLE)
        {
            p1 = new Element.Point();
            name = new String();
            comment = new String();
            radius = new Element.Number(Utils.RandomCoordinate());
            Color = color;

        }


        public override string ToString()
        {
            return $"{name}({p1.x},{p1.y}){radius}{comment}";
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if (((Element.Circle)other).p1 == this.p1 && ((Element.Circle)other).radius == this.radius) return Element.TRUE;
            return Element.FALSE;
        }
    }
    public class Arc : Element, IDrawable
    {
        public Element.String name;
        public Element.String comment;
        public Element.Point p1;
        public Element.Point p2;
        public Element.Point p3;
        public Element.Number radius;

        public Color Color { get; private set; }

        public Arc(Element.String _name, Element.Point _p1, Element.Point _p2, Element.Point _p3, Element.Number _radius, Element.String _comment, Color color) : base(ElementType.ARC)
        {
            name = _name;
            comment = _comment;
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            radius = _radius;
            Color = color;
        }
        public Arc(Color color) : base(ElementType.ARC)
        {
            p1 = new Element.Point();
            p2 = new Element.Point();
            p3 = new Element.Point();
            radius = new Element.Number(Utils.RandomCoordinate());
            name = new String();
            comment = new String();
            Color = color;
        }


        public override string ToString()
        {
            return $"{name}({p1.x},{p1.y})({p2.x},{p2.y})({p3.x},{p3.y}){radius}{comment}";
        }
        public override Number EqualTo(Element other)
        {
            if (other.Type != this.Type) return Element.FALSE;
            if (((Element.Arc)other).p1 == this.p1 && ((Element.Arc)other).p2 == this.p2 && ((Element.Arc)other).p3 == this.p3 && ((Element.Arc)other).radius == this.radius) return Element.TRUE;
            return Element.FALSE;
        }
    }
    ///<summary>Base class for sequences.</summary>
    public abstract class Sequence : Element , IEnumerable<Element>{
        protected Sequence():base(ElementType.SEQUENCE){}
        public abstract Element Count {get;}
        public abstract bool IsFinite {get;}
        public abstract bool IsEmpty {get;}
        public abstract IEnumerator<Element> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // public abstract Element Current {get;}
        // public abstract bool MoveNext();
        // public abstract void Reset();
        // public abstract void Dispose();
        // object IEnumerator.Current {get => Current;}

        ///<summary>A finite sequence of Element.</summary>
        public class Listing : Sequence {
            ///<summary>The elements of the sequence.</summary>
            List<Element> sequence;
            public Listing(List<Element> elements){
                sequence= elements;
            }
            public override bool IsFinite => true;
            public override bool IsEmpty => sequence.Count == 0;
            public override Element.Number Count =>  new Element.Number(sequence.Count);
            public override Number EqualTo(Element other)
            {
                throw new NotImplementedException();
            }
            public override IEnumerator<Element> GetEnumerator(){
                return sequence.GetEnumerator();
            }
            public override string ToString()
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                stringBuilder.Append('{');
                for(int i=0;i<sequence.Count;++i){
                    stringBuilder.Append(sequence[i].ToString());
                    if(i < sequence.Count - 1)stringBuilder.Append(',');
                }
                stringBuilder.Append('}');
                return stringBuilder.ToString();
            }
        }
    }
}