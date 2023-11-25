/*
This is the parser. It receives a token stream from the scanner and produces an abstract syntax tree.
It uses recursive descent parsing to parse the different constructs.
*/
namespace GSharpCompiler;

class Parser : GSharpCompilerComponent
{
    List<Token> tokens;
    int current = 0;//Next unprocessed token.

    public Parser(List<Token> _tokens,int maxErrorCount,ICollection<Error> errors):base(maxErrorCount,errors)
    {
        tokens = _tokens;
    }
    public override void Abort()
    {
        throw new ParserException();
    }
    public override void OnErrorFound(int line, int offset, string message, bool enforceAbort = false)
    {
        base.OnErrorFound(line, offset, message, enforceAbort);
        //If the execution doesnt stop enter recovery mode.
        throw new RecoveryModeException();
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
            try{
                Stmt tmp = ParseStmt();
                if(tmp != Stmt.EMPTY)stmts.Add(tmp);//Do not add empty statements to a program.
            }
            catch(RecoveryModeException){
                //On recovery mode the parser discards tokens until a `;` is found. After this semicolon a statement should be found.
                while(!IsAtEnd && Peek.Type != TokenType.SEMICOLON)Advance();
            }
        }

        return new Program(stmts);
    }

    private Stmt ParseStmt(){
        Stmt aux = null;
        switch(Peek.Type){
            case TokenType.SEMICOLON:
                aux = Stmt.EMPTY;
                break;
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
                OnErrorFound(Peek.Line,Peek.Offset,"Not a statement");
                break;
        }
        Consume(TokenType.SEMICOLON,"Semicolon expected after statement");
        return aux!;
    }
     private Stmt.Draw ParseDrawStmt()
    {
        int line = Peek.Line;
        int offset = Peek.Offset;
        Consume(TokenType.DRAW, "Expected `draw` keyword");
        Expr expr = ParseExpression();
        ErrorIfEmpty(expr,line,offset,"Expected non-empty expression after `draw`");

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
        ErrorIfEmpty(expr,id.Line,id.Offset,$"Assigned empty expression to constant `{id.Lexeme}`");//Rule 2
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
        
        Color color = Color.BLACK;        
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
                OnErrorFound(Peek.Line,Peek.Offset,$"Expected built-in color");
                break;
        }

        Advance();//Consume the token who holds the color.
        return new Stmt.Color(line,offset,color);
    }
    #endregion Statement parsing
    
    #region Expression parsing
    private Expr ParseExpression(){
        return ParseConditionalExpression();
    }
    private Expr ParseConditionalExpression(){
        if(Match(TokenType.IF)){
            int line = Previous.Line;
            int offset = Previous.Offset;
            Expr condition = ParseExpression();
            ErrorIfEmpty(condition,line,offset,"Expected non-empty expression for condition");
            Consume(TokenType.THEN,"Expected `then` keyword");
            Expr thenBranchExpr = ParseExpression();
            ErrorIfEmpty(thenBranchExpr,line,offset,"Expected non-empty expression after `then`");
            Consume(TokenType.ELSE,"Expected `else` keyword");
            Expr elseBranchExpr = ParseExpression();
            ErrorIfEmpty(elseBranchExpr,line,offset,"Expected non-empty expression after `else`");
            return new Expr.Conditional(line,offset,condition,thenBranchExpr,elseBranchExpr);
        }
        return ParseOrExpression();
    }
    private Expr ParseOrExpression(){
        Expr left = ParseAndExpression();
        while(Match(TokenType.OR)){
            Token operation = Previous;
            ErrorIfEmpty(left,operation.Line,operation.Offset,"Expected non-empty expression as left operand");
            Expr right = ParseAndExpression();
            ErrorIfEmpty(right,operation.Line,operation.Offset,"Expected non-empty expression as right operand");
            left = new Expr.Binary.Or(left.Line,left.Offset,operation,left,right);
        }
        return left;
    }
    private Expr ParseAndExpression(){
        Expr left = ParseEqualityExpression();
        while(Match(TokenType.AND)){
            Token operation = Previous;
            ErrorIfEmpty(left,operation.Line,operation.Offset,"Expected non-empty expression as left operand");
            Expr right = ParseEqualityExpression();
            ErrorIfEmpty(right,operation.Line,operation.Offset,"Expected non-empty expression as right operand");
            left = new Expr.Binary.And(left.Line,left.Offset,operation,left,right);
        }
        return left;
    }
    private Expr ParseEqualityExpression(){
        Expr left = ParseComparisonExpression();
        
        int counter = 0;
        Token firstOperation = Peek;

        while(Match(TokenType.EQUAL_EQUAL,TokenType.BANG_EQUAL)){
            //Rule # 9
            if(counter == 1)OnErrorFound(Previous.Line,Previous.Offset,$"Cannot use '{Previous.Lexeme}' after '{firstOperation.Lexeme}'. Consider using parenthesis and/or logical operators.");
            Token operation = Previous;
            ErrorIfEmpty(left,operation.Line,operation.Offset,"Expected non-empty expression as left operand");
            Expr right = ParseComparisonExpression();
            ErrorIfEmpty(right,operation.Line,operation.Offset,"Expected non-empty expression as right operand");
            switch(operation.Type){
                case TokenType.EQUAL_EQUAL:
                    left = new Expr.Binary.EqualEqual(left.Line,left.Offset,operation,left,right);
                    break;
                case TokenType.BANG_EQUAL:
                    left = new Expr.Binary.NotEqual(left.Line,left.Offset,operation,left,right);
                    break;
            }
            ++counter;
        }
        return left;
    }
    private Expr ParseComparisonExpression(){
        Expr left = ParseTermExpression();

        int counter = 0;//How many comparisons are being done.
        Token firstOperation = Peek;

        //Multiple chained comparisons are not supported by the grammar. So report an error if more than one is found.
        while(Match(TokenType.LESS,TokenType.LESS_EQUAL,TokenType.GREATER,TokenType.GREATER_EQUAL)){
            //Rule # 9
            if(counter == 1)OnErrorFound(Previous.Line,Previous.Offset,$"Cannot use '{Previous.Lexeme}' after '{firstOperation.Lexeme}'. Consider using parenthesis and/or logical operators.");
            Token operation = Previous;
            ErrorIfEmpty(left,operation.Line,operation.Offset,"Expected non-empty expression as left operand");
            Expr right = ParseTermExpression();
            ErrorIfEmpty(right,operation.Line,operation.Offset,"Expected non-empty expression as right operand");
            switch(operation.Type){
                case TokenType.LESS:
                    left = new Expr.Binary.Less(left.Line,left.Offset,operation,left,right);
                    break;
                case TokenType.LESS_EQUAL:
                    left = new Expr.Binary.LessEqual(left.Line,left.Offset,operation,left,right);
                    break;
                case TokenType.GREATER:
                    left = new Expr.Binary.Greater(left.Line,left.Offset,operation,left,right);
                    break;
                case TokenType.GREATER_EQUAL:
                    left = new Expr.Binary.GreaterEqual(left.Line,left.Offset,operation,left,right);
                    break;
            }
            ++counter;
        }
        return left;
    }
    private Expr ParseTermExpression(){
        Expr left = ParseFactorExpression();
        while(Match(TokenType.PLUS,TokenType.MINUS)){
            Token operation = Previous;
            ErrorIfEmpty(left,operation.Line,operation.Offset,"Expected non-empty expression as left operand");
            Expr right = ParseFactorExpression();
            ErrorIfEmpty(right,operation.Line,operation.Offset,"Expected non-empty expression as right operand");
            switch(operation.Type){
                case TokenType.PLUS:
                    left = new Expr.Binary.Sum(left.Line,left.Offset,operation,left,right);
                    break;
                case TokenType.MINUS:
                    left = new Expr.Binary.Difference(left.Line,left.Offset,operation,left,right);
                    break;
            }
        }
        return left;
    }
    private Expr ParseFactorExpression(){
        Expr left = ParsePowerExpression();
        while(Match(TokenType.STAR ,TokenType.SLASH ,TokenType.PERCENT)){
            Token operation = Previous;
            ErrorIfEmpty(left,operation.Line,operation.Offset,"Expected non-empty expression as left operand");
            Expr right = ParsePowerExpression();
            ErrorIfEmpty(right,operation.Line,operation.Offset,"Expected non-empty expression as right operand");
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
        if(Match(TokenType.CARET)){
            Token operation = Previous;
            ErrorIfEmpty(left,operation.Line,operation.Offset,"Expected non-empty expression as left operand");
            Expr right = ParsePowerExpression();
            ErrorIfEmpty(right,operation.Line,operation.Offset,"Expected non-empty expression as right operand");
            return new Expr.Binary.Power(left.Line,left.Offset,operation,left,right);
        }
        return left;
    }
    private Expr ParseUnaryExpression(){
        Expr expr;
        switch(Peek.Type){
            case TokenType.BANG:
                Advance();//Consume the operator
                expr = ParseUnaryExpression();
                ErrorIfEmpty(expr,Previous.Line,Previous.Offset,"Expected non-empty expression as operand");
                return new Expr.Unary.Not(Previous.Line,Previous.Offset,expr);
            case TokenType.MINUS:
                Advance();//Consume the operator
                expr = ParseUnaryExpression();
                ErrorIfEmpty(expr,Previous.Line,Previous.Offset,"Expected non-empty expression as operand");
                return new Expr.Unary.Minus(Previous.Line,Previous.Offset,expr);
            default:
                return ParseVariableExpression();
        }
    }
    private Expr ParseVariableExpression(){
        if(Match(TokenType.ID)){
            Token id = Previous;
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
        OnErrorFound(Previous.Line, Previous.Offset + Previous.Lexeme.Length, message);
        //Unreachable code
        return Peek;
    }
    //If the current token match any of the given tokens advance an return true, if not return false and keep current.
    //It is a conditional advance.
    private bool Match(params TokenType[] types){
        foreach(TokenType type in types){
            if(type == Peek.Type){
                Advance();
                return true;
            }
        }
        return false;
    }
    ///<summary>If <c>expr</c> is the EMPTY expression its detected as an error.</summary>
    private void ErrorIfEmpty(Expr expr,int line,int offset,string message,bool enforceAbort = false){
        if(expr == Expr.EMPTY) OnErrorFound(line,offset,message,enforceAbort);
    }
    #endregion
}