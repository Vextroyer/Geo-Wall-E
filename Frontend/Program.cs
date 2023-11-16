/*
This class represents a G# program.
A program is a list of statements.
*/

namespace Frontend;

class Program{
    public List<Stmt> Stmts {get; private set;}//How can i give access to the statements to read but not to write.
    public Program(List<Stmt> _stmts){
        Stmts = _stmts;
    }
}