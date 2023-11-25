/*
On this class are defined the different objects that exist on a G# program execution.
*/
namespace GSharpCompiler;

//Represents G# objects that can be drawed on screen. Not every object can be drawed.
public interface IDrawable{}

public enum ElementType{
    UNDEFINED,
    NUMBER,
    STRING,
    POINT
}

//Base class for all elements that exist during runtime.
public abstract class Element{
    //The type of the Element.
    public ElementType Type {get; private set;}

    //Constant elements of the different types
    public static Element.Number NUMBER = new Element.Number(0);
    public static Element.String STRING = new Element.String("");
    public static Element.Point POINT = new Element.Point(STRING,NUMBER,NUMBER,STRING);
    //Boolean values are represented with numbers
    public static Element.Number TRUE = new Element.Number(1);
    public static Element.Number FALSE = new Element.Number(0);

    protected Element(ElementType type){
        Type = type;
    }
    //Equality operator
    public abstract Element.Number EqualTo(Element other);
    public Element.Number NotEqualTo(Element other){
        if(this.EqualTo(other) == Element.TRUE)return Element.FALSE;
        return Element.TRUE;
    }

    //Represents a real number.
    public class Number:Element{
        float value;
        public float Value {
            get
            {
                return value;
            }
        }
        public Number(float _value):base(ElementType.NUMBER){
            value = _value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
        public override Number EqualTo(Element other)
        {
            if(other.Type != this.Type)return Element.FALSE;
            if(((Element.Number)other).value == this.value)return Element.TRUE;
            return Element.FALSE;
        }
        static public Element.Number operator -(Element.Number number){
            return new Element.Number(- number.value);
        }
        static public Element.Number operator ^(Element.Number left,Element.Number right){
            return new Element.Number((float) Math.Pow(left.value,right.value));
        }
        static public Element.Number operator *(Element.Number left,Element.Number right){
            return new Element.Number(left.value * right.value);
        }
        static public Element.Number operator /(Element.Number left,Element.Number right){
            return new Element.Number(left.value / right.value);
        }
        static public Element.Number operator %(Element.Number left,Element.Number right){
            return new Element.Number(left.value % right.value);
        }
        static public Element.Number operator +(Element.Number left,Element.Number right){
            return new Element.Number(left.value + right.value);
        }
        static public Element.Number operator -(Element.Number left,Element.Number right){
            return new Element.Number(left.value - right.value);
        }
        static public Element.Number operator <(Element.Number left,Element.Number right){
            if(left.value < right.value)return Element.TRUE;
            return Element.FALSE;
        }
        static public Element.Number operator <=(Element.Number left,Element.Number right){
            if(left.value <= right.value)return Element.TRUE;
            return Element.FALSE;
        }
        static public Element.Number operator >(Element.Number left,Element.Number right){
            if(left.value > right.value)return Element.TRUE;
            return Element.FALSE;
        }
        static public Element.Number operator >=(Element.Number left,Element.Number right){
            if(left.value >= right.value)return Element.TRUE;
            return Element.FALSE;
        }
    }

    //Represents a string.
    public class String:Element{
        string value;
        public string Value {
            get
            {
                return value;
            }
        }
        public String(string _value):base(ElementType.STRING){
            value = _value;
        }
        public override string ToString()
        {
            return value;
        }
        public override Number EqualTo(Element other)
        {
            if(other.Type != this.Type)return Element.FALSE;
            if(((Element.String)other).value == this.value)return Element.TRUE;
            return Element.FALSE;
        }
    }

    //Represents a point on a 2D rectangular coordinate system.
    public class Point:Element,IDrawable{
        Element.String name;
        Element.String comment;
        Element.Number x;
        Element.Number y;

        public Point(Element.String _name,Element.Number _x,Element.Number _y,Element.String _comment):base(ElementType.POINT){
            name = _name;
            comment = _comment;
            x = _x;
            y = _y;
        }

        public override string ToString()
        {
            return $"{name}({x},{y}){comment}";
        }
        public override Number EqualTo(Element other)
        {
            if(other.Type != this.Type)return Element.FALSE;
            if(((Element.Point)other).x == this.x && ((Element.Point)other).y == this.y)return Element.TRUE;
            return Element.FALSE;
        }
    }
};