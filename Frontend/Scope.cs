/*
The state of the program is made of a nested scope structure.
An scope is where variables and functions are stored.
*/

namespace Frontend;

public class Scope{
    Scope? parent;

    Dictionary<string,Element> elements = new Dictionary<string, Element>();

    public Scope(Scope? _parent = null){
        parent = _parent;
        
    }

    //Associate the given varName with the given element.
    //TODO-Treat redeclaration as an error
    public void Set(string varName, Element element){
        elements.Add(varName,element);
    }
    //Return the element associated with the given varName
    //TODO-SEARCH in the parents scopes
    public Element Get(string varName){
        return elements[varName];
    }
}