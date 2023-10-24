/*
Define exceptions to be used by the program.
*/
namespace Frontend;
public class ExtendedException : Exception{
    public int Line {get; private set;}
    public int Offset {get; private set;}

    public ExtendedException(int line,int offset,string message = ""):base(message){
        Line = line;
        Offset = offset;
    }
}