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
    #region Statement parsing
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
        Stmt aux;
        switch(Peek.Type){
            case TokenType.POINT:
                aux = ParsePointStmt();
                break;
            case TokenType.ID:
                aux = ParseConstantDeclaration();
                break;
            case TokenType.PRINT:
                aux = ParsePrintStmt();
                break;
            default:
                throw new ExtendedException(Peek.Line,Peek.Offset,"Not a statement");
        }
        Consume(TokenType.SEMICOLON,"Semicolon expected after statement");
        return aux;
    }
    private Stmt.Point ParsePointStmt(){
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.POINT,"Expected `point` keyword");

        //Since parsing phase points get their coordinates.
        float x = Utils.RandomCoordinate();
        float y = Utils.RandomCoordinate();
        //A parenthesis after a point keyword means a constructor with the point coordinates.
        if(Peek.Type == TokenType.LEFT_PAREN){
            Consume(TokenType.LEFT_PAREN);
            x = (float) Consume(TokenType.NUMBER,"Expected NUMBER as first parameter").Literal!;
            Consume(TokenType.COMMA,"Expected comma `,`");
            y = (float) Consume(TokenType.NUMBER,"Expected NUMBER as second parameter").Literal!;
            Consume(TokenType.RIGHT_PAREN,"Expected `)`");
        }

        Token id = Consume(TokenType.ID,"Expected identifier");
        
        //If the current token is an string then its a comment on the point.
        string comment = "";
        if(Peek.Type == TokenType.STRING)comment =(string) Advance().Literal!;

        return new Stmt.Point(_line,_offset,id,new Element.Number(x),new Element.Number(y),new Element.String(comment));
    }
    private Stmt.ConstantDeclaration ParseConstantDeclaration(){
        Token id = Consume(TokenType.ID,"Expected identifier");
        Consume(TokenType.EQUAL,"Expected `=`");
        Expr expr = ParseExpression();
        return new Stmt.ConstantDeclaration(id,expr);
    }
    private Stmt.Print ParsePrintStmt(){
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.PRINT);
        return new Stmt.Print(_line,_offset,ParseExpression());
    }
    #endregion Statement parsing
    
    #region Expression parsing
    private Expr ParseExpression(){
        return ParseVariableExpression();
    }
    private Expr ParseVariableExpression(){
        if(Peek.Type == TokenType.ID){
            Token id = Consume(TokenType.ID);
            return new Expr.Variable(id);
        }
        return ParsePrimaryExpression();
    }
    private Expr ParsePrimaryExpression(){
        switch(Peek.Type){
            case TokenType.NUMBER:
                return new Expr.Number(Peek.Line,Peek.Offset,(float) Advance().Literal!);
            case TokenType.STRING:
                return new Expr.String(Peek.Line,Peek.Offset,(string) Advance().Literal!);
            case TokenType.LEFT_PAREN://Grouping expressions
                Consume(TokenType.LEFT_PAREN);
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN,"Expected `)`");
                return expr;
            default :  return Expr.EMPTY;//The empty expression
        }
    }
    #endregion Expression parsing

    #region  Auxiliary Methods
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
    private Token Consume(TokenType type,string message="(-_-)"){
        if(Peek.Type == type)return Advance();
        //Heuristically report the location of the error as the end of the previous token.
        throw new ExtendedException(Previous.Line,Previous.Offset + Previous.Lexeme.Length,message);
    }
    #endregion
}