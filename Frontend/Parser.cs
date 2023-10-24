/*
This is the parser.
*/
namespace Frontend;

public class Parser{
    List<Token> tokens;
    int current = 0;//Next unprocessed token.

    public Parser(List<Token> _tokens){
        tokens = _tokens;
    }

    public Program Parse(){
        return ParseProgram();
    }

    //Recursive descent parsing methods based on the grammar defined on Grammar file.
    private Program ParseProgram(){
        List<Stmt> stmts = new List<Stmt>();
        
        while(!IsAtEnd){
            stmts.Add(ParseStmt());
        }

        return new Program(stmts);
    }
    
    private Stmt ParseStmt(){
        Stmt aux = ParsePointStmt();
        Consume(TokenType.SEMICOLON,"Semicolon expected after statement");
        return aux;
    }

    private Stmt.Point ParsePointStmt(){
        Consume(TokenType.POINT,"Expected `point` keyword");
        Token id = Consume(TokenType.ID,"Expected identifier");
        //If the current token is an string then its a comment on the point.
        string comment = "";
        if(Peek.Type == TokenType.STRING)comment =(string) Advance().Literal!;

        return new Stmt.Point(id,comment);
    }

    private bool IsAtEnd {get => Peek.Type == TokenType.EOF;}
    //Return the current token without moving current,which without consuming its.
    private Token Peek {get => tokens[current];}
    //Return the current token and move current one position ahead, this is called consume the token.
    private Token Advance(){
        ++current;
        return tokens[current - 1];
    }
    //Return the last processed token.
    private Token Previous {get => tokens[current - 1];}
    //If the current token is of the expected type consume it, if not throw an exception.
    private Token Consume(TokenType type,string message){
        if(Peek.Type == type)return Advance();
        //Heuristically report the location of the error as the end of the previous token.
        throw new ExtendedException(Previous.Line,Previous.Offset + Previous.Lexeme.Length,message);
    }
}