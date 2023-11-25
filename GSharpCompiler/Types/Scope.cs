/*
The state of the program is made of a nested scope structure.
An scope is where variables and functions are stored.
Variables are classified on two types: arguments and constants.
Arguments can change the value they are associated to.
Constants cant change that value.
*/

namespace GSharpCompiler;

/*
Its convenient to have a generic type for the scope because in the case of the interpreter
the variables are asscociated with elements, but in the case of the TypeChecker the variables
are associated with its type.
*/

class Scope<U>{
    Scope<U>? parent;
    Dictionary<string,U> elements = new Dictionary<string, U>();
    HashSet<string> constants = new HashSet<string>();
    public Scope(Scope<U>? _parent = null){
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
    public void SetArgument(string varName, U element){
        elements.Add(varName,element);
    }
    public void SetConstant(string varName, U element){
        SetArgument(varName,element);
        constants.Add(varName);
    }
    //Return the element associated with the given varName. The method HasBinding should be called
    //before calling get setting searchParent to true.
    public U Get(string varName){
        if(HasBinding(varName))return elements[varName];
        if(HasParent)return parent!.Get(varName);
        throw new Exception($"Tried to acces to inexsitent variable {varName}");
    }
}