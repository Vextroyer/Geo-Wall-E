/*
Represents a scope.
*/

namespace Frontend;

class Scope{
    Scope? parent;

    Dictionary<string,Element> elements = new Dictionary<string, Element>();

    public Scope(Scope? _parent = null){
        parent = _parent;
        
    }

    //Associate the given varName with the given element.
    public void Set(string varName, Element element){
        elements.Add(varName,element);
    }
    //Return the element associated with the given varName
    public Element Get(string varName){
        return elements[varName];
    }
}