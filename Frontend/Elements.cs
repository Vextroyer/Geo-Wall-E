/*
On this class are defined the different objects that can exist on a G# program.
*/
namespace Frontend;

abstract class Element{};

class Point:Element{
    string name;
    string comment;

    public Point(string _name = "",string _comment = ""){
        this.name = _name;
        this.comment = _comment;
    }
}