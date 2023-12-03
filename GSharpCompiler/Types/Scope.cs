/*
The state of the program is made of a nested scope structure.
An scope is where variables and functions are stored.
Variables are classified on two types: arguments and constants.
Arguments can change the value they are associated to.
Constants cant change that value.
Functions are arguments. Functions as arguments are represented by a list
of functions which share the same name but differ on their arities.
*/

namespace GSharpCompiler;

class Scope{
    Scope? parent;
    Dictionary<string,Element> elements = new Dictionary<string, Element>();
    HashSet<string> constants = new HashSet<string>();
    public Scope(Scope? _parent = null){
        parent = _parent;
    }
    private bool HasParent { get => parent != null; }
    ///<summary>Determine wheter the given name is associated to a constant.</summary>
    ///<returns>True if varName is associted to a constant in this or any enclosing scope. False otherwise.</returns>
    private bool IsConstant(string varName){
        if(constants.Contains(varName))return true;
        if(HasParent)return parent!.IsConstant(varName);
        return false;
    }
    ///<summary>Associates varName and newElement. If varName is associated to a constant or varName is declared on this scope then a ScopeException is launched.</summary>
    public void SetArgument(string varName, Element newElement){
        if(IsConstant(varName))throw new ScopeException($"Redeclaration of constant `{varName}`");//Rule # 1
        //There two cases must be handled , varName is a new variable or varName already exist as a variable.
        if(elements.ContainsKey(varName)){
            //In the case that varName already exist there are two cases.
            
            if(elements[varName].Type == ElementType.FUNCTION_LIST){
                //varName is associated to a function
                //Two more cases must be handled, the case when the given element is non-function and the case when is a function
                if(newElement.Type == ElementType.FUNCTION){
                    Element.FunctionList elementAssociatedToVarNameAsFunctionList = (elements[varName] as Element.FunctionList)!;
                    Element.Function newElementAsFunction = (newElement as Element.Function)!;
                    //If the newElement is a function then there are two more cases.
                    //A function with this same arity exist , which is considered a redeclaration.
                    if( elementAssociatedToVarNameAsFunctionList.ContainsArity(newElementAsFunction.Arity) ) throw new ScopeException($"A definition of function `{varName}` with `{newElementAsFunction.Arity}` arguments already exist");
                    //Or this function has a different arity, then its added to the functon list.
                    elementAssociatedToVarNameAsFunctionList.Add(newElementAsFunction);
                }
                else{
                    //If the element is not a function then this is a redeclaration. Rule # 3.
                    throw new ScopeException($"Variable `{varName}` is declared twice on the same scope");
                }
            }
            else{
                //varName is associated to a non-function element.
                //On this case we have two declarations on the same scope. Rule # 3. This is an error.
                throw new ScopeException($"Variable `{varName}` is declared twice on the same scope");
            }
        }
        else{
            //In the case var name doesnt exist two more cases will be handled.The case of adding a simple argument or the case of adding a function.
            if(newElement.Type == ElementType.FUNCTION){
                //Adding a function.
                //Create a new function list.
                Element.FunctionList newFunction = new Element.FunctionList();
                //Add the function to the list.
                newFunction.Add((Element.Function) newElement);
                //Bind varName and the function.
                elements.Add(varName,newFunction);
            }
            else{
                //Adding a non-function argument.
                elements.Add(varName,newElement);
            }
        }
    }
    ///<summary>Associate varName to the element as a constant.</summary>
    public void SetConstant(string varName, Element element){
        SetArgument(varName,element);
        constants.Add(varName);
    }
    ///<summary>Retrieve the element associated with varName. If arity is less than zero the element is considered non-function. If arity is zero or greater then the element is considered function.</summary>
    public Element Get(string varName,int arity = -1){
        //Check that the element is in this scope.
        if(elements.ContainsKey(varName)){
            Element element = elements[varName];
            if(element.Type == ElementType.FUNCTION_LIST){
                Element.FunctionList elementAsFunctionList = (element as Element.FunctionList)!;
                //If the declared element is a function it cant be used as a variable
                if(arity < 0){
                    throw new ScopeException($"Attempted to use function `{varName}` as a variable");
                }
                //If no signature match this arity then this is an error
                if(!elementAsFunctionList.ContainsArity(arity))throw new ScopeException($"No overload of function `{varName}` takes {arity} parameters");
                //Use the function that matches the signature.
                return elementAsFunctionList.GetFunction(arity);
            }
            else{
                //If the declared element is not a function it cant be used as a function
                if(arity >= 0)throw new ScopeException("Attempted to use variable `{varName}` as a function");
                //Return the element
                return element;
            }
        }
        //Check in the parent scope.
        if(HasParent)return parent!.Get(varName,arity);
        //The element is not declared.
        if(arity < 0) throw new ScopeException($"Variable `{varName}` not declared");
        else throw new ScopeException($"Function `{varName}` with `{arity}` parameters is not declared");
    }
}