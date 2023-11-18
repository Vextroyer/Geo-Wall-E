/*
This is the parser. It receives a token stream from the scanner and produces an abstract syntax tree.
It uses recursive descent parsing to parse the different constructs.
*/
namespace Frontend;

class Parser
{
    List<Token> tokens;
    int current = 0;//Next unprocessed token.

    public Parser(List<Token> _tokens)
    {
        tokens = _tokens;
    }

    #region Statement parsing
    public Program Parse(){
        return ParseProgram();
    }
    //Recursive descent parsing methods based on the grammar defined on Grammar file.
    private Program ParseProgram()
    {
        List<Stmt> stmts = new List<Stmt>();

        while (!IsAtEnd)
        {
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
            case TokenType.DRAW:
                 aux=ParseDrawStmt();
                break;
            case TokenType.COLOR:
            case TokenType.RESTORE:
                aux = ParseColorStmt();
                break;
            default:
                throw new ExtendedException(Peek.Line,Peek.Offset,"Not a statement");
        }
        Consume(TokenType.SEMICOLON,"Semicolon expected after statement");
        return aux;
    }
     private Stmt.Draw ParseDrawStmt()
    {
        int line = Peek.Line;
        int offset = Peek.Offset;
        Consume(TokenType.DRAW, "Expected `draw` keyword");
        Expr expr = ParseExpression();

        return new Stmt.Draw(line,offset,expr);

    }
    private Stmt.Point ParsePointStmt(){
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.POINT,"Expected `point` keyword");

        //Since parsing phase points get their coordinates.
        float x = Utils.RandomCoordinate();
        float y = Utils.RandomCoordinate();
        //A parenthesis after a point keyword means a constructor with the point coordinates.
        if (Peek.Type == TokenType.LEFT_PAREN)
        {
            Consume(TokenType.LEFT_PAREN);
            x = (float)Consume(TokenType.NUMBER, "Expected NUMBER as first parameter").Literal!;
            Consume(TokenType.COMMA, "Expected comma `,`");
            y = (float)Consume(TokenType.NUMBER, "Expected NUMBER as second parameter").Literal!;
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        }

        Token id = Consume(TokenType.ID, "Expected identifier");

        //If the current token is an string then its a comment on the point.
        string comment = "";
        if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;

        return new Stmt.Point(_line,_offset,id,new Element.Number(x),new Element.Number(y),new Element.String(comment));
    }
    private Stmt.ConstantDeclaration ParseConstantDeclaration(){
        Token id = Consume(TokenType.ID,"Expected identifier");
        Consume(TokenType.EQUAL,"Expected `=`");
        Expr expr = ParseExpression();
        if(expr == Expr.EMPTY)throw new ExtendedException(id.Line,id.Offset,$"Assigned empty expression to constant `{id.Lexeme}`");//Rule 2
        return new Stmt.ConstantDeclaration(id,expr);
    }
    private Stmt.Print ParsePrintStmt(){
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.PRINT);
        return new Stmt.Print(_line,_offset,ParseExpression());
    }
    private Stmt.Color ParseColorStmt(){
        int line = Peek.Line;
        int offset = Peek.Offset;
        
        //If the keyword `restore` is found the color doesnt matter
        if(Peek.Type == TokenType.RESTORE){
            Consume(TokenType.RESTORE);
            return new Stmt.Color(line,offset,Color.BLACK,true);
        }
        
        Color color;        
        Consume(TokenType.COLOR);
        //If the `color` keyword is found , a built-in color must be provided.
        switch(Peek.Type){
            case TokenType.COLOR_BLACK:
                color = Color.BLACK;
                break;
            case TokenType.COLOR_BLUE:
                color = Color.BLUE;
                break;
            case TokenType.COLOR_CYAN:
                color = Color.CYAN;
                break;
            case TokenType.COLOR_GRAY:
                color = Color.GRAY;
                break;
            case TokenType.COLOR_GREEN:
                color = Color.GREEN;
                break;
            case TokenType.COLOR_MAGENTA:
                color = Color.MAGENTA;
                break;
            case TokenType.COLOR_RED:
                color = Color.RED;
                break;
            case TokenType.COLOR_WHITE:
                color = Color.WHITE;
                break;
            case TokenType.COLOR_YELLOW:
                color = Color.YELLOW;
                break;
            default:
                throw new ExtendedException(Peek.Line,Peek.Offset,$"Expected built-in color");
        }

        Advance();//Consume the token who holds the color.
        return new Stmt.Color(line,offset,color);
    }
    #endregion Statement parsing
    
    #region Expression parsing
    private Expr ParseExpression(){
        return ParseFactorExpression();
    }
    private Expr ParseFactorExpression(){
        Expr left = ParsePowerExpression();
        while(Peek.Type == TokenType.STAR || Peek.Type == TokenType.SLASH || Peek.Type == TokenType.PERCENT){
            Token operation = Advance();
            Expr right = ParsePowerExpression();
            switch(operation.Type){
                case TokenType.STAR:
                    left = new Expr.Binary.Product(left.Line,left.Offset,operation,left,right);
                    break;
                case TokenType.SLASH:
                    left = new Expr.Binary.Division(left.Line,left.Offset,operation,left,right);
                    break;
                case TokenType.PERCENT:
                    left = new Expr.Binary.Modulus(left.Line,left.Offset,operation,left,right);
                    break;
            }
        }
        return left;
    }
    private Expr ParsePowerExpression(){
        Expr left = ParseUnaryExpression();
        if(Peek.Type == TokenType.CARET){
            Token operation = Advance();//Consume the caret
            Expr right = ParsePowerExpression();
            return new Expr.Binary.Power(left.Line,left.Offset,operation,left,right);
        }
        return left;
    }
    private Expr ParseUnaryExpression(){
        switch(Peek.Type){
            case TokenType.NOT:
                Advance();//Consume the operator
                return new Expr.Unary.Not(Previous.Line,Previous.Offset,ParseUnaryExpression());
            case TokenType.MINUS:
                Advance();//Consume the operator
                return new Expr.Unary.Minus(Previous.Line,Previous.Offset,ParseUnaryExpression());
            default:
                return ParseVariableExpression();
        }
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
      
    private bool IsAtEnd { get => Peek.Type == TokenType.EOF; }

    //Return the current token without moving current,which without consuming its.
    private Token Peek { get => tokens[current]; }
    //Return the current token and move current one position ahead, this is called consume the token.
    private Token Advance()
    {
        ++current;
        return tokens[current - 1];
    }
    //Return the last processed token.
    private Token Previous { get => tokens[current - 1]; }
    //If the current token is of the expected type consume it, if not throw an exception.
    private Token Consume(TokenType type, string message = "(-_-)")
    {
        if (Peek.Type == type) return Advance();
        //Heuristically report the location of the error as the end of the previous token.
        throw new ExtendedException(Previous.Line, Previous.Offset + Previous.Lexeme.Length, message);
    }
    #endregion
}