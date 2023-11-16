/*
On this class are defined the different objects that exist on a G# program execution.
*/
namespace Frontend;

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

    protected Element(ElementType type){
        Type = type;
    }

    //Represents a real number.
    public class Number:Element{
        float value;
        public Number(float _value):base(ElementType.NUMBER){
            value = _value;
        }
        public override string ToString()
        {
            return value.ToString();
        }
    }

    //Represents a string.
    public class String:Element{
        string value;
        public String(string _value):base(ElementType.STRING){
            value = _value;
        }
        public override string ToString()
        {
            return value;
        }
    }

    //Represents a point on a 2D rectangular coordinate system.
    public class Point:Element,IDrawable{
        Element.String name;
        Element.String comment;
        Element.Number x;
        Element.Number y;

        public Point(Element.String _name,Element.Number _x,Element.Number _y,Element.String _comment):base(ElementType.STRING){
            name = _name;
            comment = _comment;
            x = _x;
            y = _y;
        }

        public override string ToString()
        {
            return $"{name}({x},{y}){comment}";
        }
    }
};