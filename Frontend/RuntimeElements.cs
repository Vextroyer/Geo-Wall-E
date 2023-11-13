/*
On this class are defined the different objects that exist on a G# program execution.
*/
namespace Frontend;

//Base class for all elements that exist during runtime.
abstract class Element{};

//This class represents points on a 2D rectangular coordinate system.
class Point:Element{
    string name;
    string comment;
    float x;
    float y;

    public Point(string _name = "",string _comment = "",float _x = 0, float _y = 0){
        this.name = _name;
        this.comment = _comment;
        x = _x;
        y = _y;
    }
}