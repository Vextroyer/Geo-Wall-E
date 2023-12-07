namespace GSharpCompiler;

///<summary>Define operations over Elements.</summary>
static class OperationTable{
    ///<summary>Different overcharges of an operation.</summary>
    private static Dictionary<string,List<Delegate>> operationTable = new Dictionary<string,List<Delegate>>();
    ///<summary>Apply operation over the parameters.</summary>
    public static Element Operate(string operation,params Element[] parameters){
        try{
            foreach(Delegate op in operationTable[operation]){
                try{
                    return (op.DynamicInvoke(parameters) as Element)!;
                }
                catch(System.Reflection.TargetParameterCountException){}//Wrong number of parameters.
                catch(System.ArgumentException){}//wrong type of parameters.
                catch(InvalidCastException){}//return type is not an element. The operation is ill defined.
            }
            //Cant operate with this.
            throw new InvalidOperationException("Invalid operation");
        }
        catch(KeyNotFoundException){
            //If this operation is not defined this operation is invalid.
            throw new InvalidOperationException("Operation doesnt exist");
        }
    }
    ///<summary>Associate a behaviour with a given operator. The delegate must return an Element and must operate over elements.</summary>
    public static void Register(string operation,Delegate behaviour){
        if(!operationTable.ContainsKey(operation))operationTable.Add(operation,new List<Delegate>());
        operationTable[operation].Add(behaviour);
    }

    ///<summary>Initialize the basic operations.</summary>
    static OperationTable(){
        #region +
        Register("+",(Element.Number a,Element.Number b) => new Element.Number(a.Value + b.Value));
        Register("+",(Element.Measure a,Element.Measure b) => new Element.Measure(a.Value + b.Value));
        #endregion +
        #region -
        Register("-",(Element.Number a,Element.Number b) => new Element.Number(a.Value - b.Value));
        Register("-",(Element.Measure a,Element.Measure b) => new Element.Measure(float.Abs(a.Value - b.Value)));
        Register("-",(Element.Number a) => new Element.Number(-a.Value));
        #endregion -
        #region /
        Register("/",(Element.Number a,Element.Number b) => {
            if(Utils.IsZero(b.Value))throw new DivideByZeroException();
            return new Element.Number(a.Value / b.Value);
        }
        );
        Register("/",(Element.Measure a,Element.Measure b) => {
            if(Utils.IsZero(b.Value))throw new DivideByZeroException();
            return new Element.Number(a.Value / b.Value);
        }
        );
        #endregion /
        #region %
        Register("%",(Element.Number a,Element.Number b) => {
            if(Utils.IsZero(b.Value))throw new DivideByZeroException();
            return new Element.Number(a.Value % b.Value);
        }
        );
        #endregion %
        #region *
        Register("*",(Element.Number a,Element.Number b) => new Element.Number(a.Value * b.Value));
        Register("*",(Element.Measure a,Element.Number b) => new Element.Measure(a.Value * float.Abs(b.Value)));
        Register("*",(Element.Number a,Element.Measure b) => new Element.Measure(b.Value * float.Abs(a.Value)));
        #endregion *
        #region ^
        Register("^",(Element.Number a,Element.Number b) => new Element.Number(float.Pow(a.Value,b.Value)));
        #endregion ^
        #region <
        Register("<",(Element.Number a,Element.Number b) => {if(Utils.Compare(a.Value,b.Value) < 0)return Element.TRUE; return Element.FALSE;});
        Register("<",(Element.Measure a,Element.Measure b) => {if(Utils.Compare(a.Value,b.Value) < 0)return Element.TRUE; return Element.FALSE;});
        #endregion <
        #region <=
        Register("<=",(Element.Number a,Element.Number b) => {if(Utils.Compare(a.Value,b.Value) <= 0)return Element.TRUE; return Element.FALSE;});
        Register("<=",(Element.Measure a,Element.Measure b) => {if(Utils.Compare(a.Value,b.Value) <= 0)return Element.TRUE; return Element.FALSE;});
        #endregion <=
        #region >
        Register(">",(Element.Number a,Element.Number b) => {if(Utils.Compare(a.Value,b.Value) > 0)return Element.TRUE; return Element.FALSE;});
        Register(">",(Element.Measure a,Element.Measure b) => {if(Utils.Compare(a.Value,b.Value) > 0)return Element.TRUE; return Element.FALSE;});
        #endregion >
        #region >=
        Register(">=",(Element.Number a,Element.Number b) => {if(Utils.Compare(a.Value,b.Value) >= 0)return Element.TRUE; return Element.FALSE;});
        Register(">=",(Element.Measure a,Element.Measure b) => {if(Utils.Compare(a.Value,b.Value) >= 0)return Element.TRUE; return Element.FALSE;});
        #endregion >=
        #region !
        Register("!",(Element a) =>
        {
            if(a is Element.Number && Utils.IsZero((a as Element.Number)!.Value))return Element.TRUE;
            if(a is Element.Measure && Utils.IsZero((a as Element.Measure)!.Value))return Element.TRUE;
            if(a is Element.Undefined)return Element.TRUE;
            return Element.FALSE;
        }
        );
        #endregion
    }
}
