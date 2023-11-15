/*
The state of the program is made of a nested scope structure.
An scope is where variables and functions are stored.
Variables are classified on two types: arguments and constants.
Arguments can change the value they are associated to.
Constants cant change that value.
*/

namespace Frontend;

public class Scope{
    Scope? parent;
    Dictionary<string,Element> elements = new Dictionary<string, Element>();
    HashSet<string> constants = new HashSet<string>();
    public Scope(Scope? _parent = null){
        parent = _parent;
    }
    private bool HasParent { get => parent != null; }
    //Returns true if the variable associated to varName is a constant.
    public bool IsConstant(string varName){
        if(constants.Contains(varName))return true;
        if(HasParent)return parent!.IsConstant(varName);
        return false;
    }
    //Determines if exist a variable associated to varName. If searchParent is true, it searches on the
    //parent scopes, if not it only search on the current scope.
    public bool HasBinding(string varName,bool searchParent = false){
        if(elements.ContainsKey(varName))return true;
        if(searchParent && HasParent)return parent!.HasBinding(varName,true);
        return false;
    }
    //Associates varName with the given element.If exists is overwritted.
    public void SetArgument(string varName, Element element){
        elements.Add(varName,element);
    }
    public void SetConstant(string varName, Element element){
        SetArgument(varName,element);
        constants.Add(varName);
    }
    //Return the element associated with the given varName. The method HasBinding should be called
    //before calling get setting searchParent to true.
    public Element Get(string varName){
        if(HasBinding(varName))return elements[varName];
        if(HasParent)return parent!.Get(varName);
        throw new Exception($"Tried to acces to inexsitent variable {varName}");
    }
}