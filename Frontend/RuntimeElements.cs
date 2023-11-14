/*
On this class are defined the different objects that exist on a G# program execution.
*/
namespace Frontend;

//Base class for all elements that exist during runtime.
public abstract class Element{

    //This class represents points on a 2D rectangular coordinate system.
    public class Point:Element{
        string name;
        string comment;
        float x;
        float y;

        public Point(string _name,float _x,float _y,string _comment){
            name = _name;
            comment = _comment;
            x = _x;
            y = _y;
        }
    }

};