/*
This is the parser. It receives a token stream from the scanner and produces an abstract syntax tree.
It uses recursive descent parsing to parse the different constructs.
*/
namespace GSharpCompiler;

class Parser : GSharpCompilerComponent
{
    List<Token> tokens;
    int current = 0;//Next unprocessed token.

    public Parser(List<Token> _tokens, int maxErrorCount, ICollection<Error> errors) : base(maxErrorCount, errors)
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
    public Program Parse()
    {
        return ParseProgram();
    }
    //Recursive descent parsing methods based on the grammar defined on Grammar file.
    private Program ParseProgram()
    {
        return new Program(ParseStmtList());
    }
    private Stmt.StmtList ParseStmtList(TokenType stopAtThisType = TokenType.EOF)
    {
        Stmt.StmtList stmts = new Stmt.StmtList(Peek.Line, Peek.Offset);
        while (!IsAtEnd && Peek.Type != stopAtThisType)
        {
            try
            {
                Stmt tmp = ParseStmt();
                if (tmp != Stmt.EMPTY) stmts.Add(tmp);//Do not add empty statements to a program.
            }
            catch (RecoveryModeException)
            {
                //On recovery mode the parser discards tokens until a `;` is found. After this semicolon a statement should be found.
                //However, inside of a let-in statement list, the discard can be done until the `in` keyword is found.
                while (!IsAtEnd && Peek.Type != TokenType.SEMICOLON && Peek.Type != stopAtThisType) Advance();
            }
        }
        if (Peek.Type != stopAtThisType) OnErrorFound(Previous.Line, Previous.Offset, $"Unexpected EOF encountered, could not reach token of type {stopAtThisType}", true);
        return stmts;
    }
    private Stmt ParseStmt()
    {
        Stmt aux = null;
        switch (Peek.Type)
        {
            case TokenType.SEMICOLON:
                aux = Stmt.EMPTY;
                break;
            case TokenType.POINT:
                aux = ParsePointStmt();
                break;
            case TokenType.LINE:
                aux = ParseLinesStmt();
                break;
            case TokenType.SEGMENT:
                aux = ParseSegmentStmt();
                break;
            case TokenType.RAY:
                aux = ParseRayStmt();
                break;
            case TokenType.ID:
                aux = ParseConstantDeclaration();
                break;
            case TokenType.PRINT:
                aux = ParsePrintStmt();
                break;
            case TokenType.DRAW:
                aux = ParseDrawStmt();
                break;
            case TokenType.COLOR:
            case TokenType.RESTORE:
                aux = ParseColorStmt();
                break;
            case TokenType.EVAL:
                aux = ParseEvalStmt();
                break;
            default:
                OnErrorFound(Peek.Line, Peek.Offset, "Not a statement");
                break;
        }
        Consume(TokenType.SEMICOLON, "Semicolon expected after statement");
        return aux!;
    }
    private Stmt.Draw ParseDrawStmt()
    {
        int line = Peek.Line;
        int offset = Peek.Offset;
        Consume(TokenType.DRAW, "Expected `draw` keyword");
        Expr expr = ParseExpression();
        ErrorIfEmpty(expr, line, offset, "Expected non-empty expression after `draw`");

        return new Stmt.Draw(line, offset, expr);

    }
    private Stmt.Point ParsePointStmt()
    {
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.POINT, "Expected `point` keyword");

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

        return new Stmt.Point(_line, _offset, id, new Element.Number(x), new Element.Number(y), new Element.String(comment));
    }

    private Stmt.Lines ParseLinesStmt()
    {
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.LINE, "Expected `line` keyword");


        //Expr p1 = new Expr.Variable();
        //Element.Point p2 = new Element.Point();

        //A parenthesis after a point keyword means a constructor with the points.

        Consume(TokenType.LEFT_PAREN);
        // p1 = (Element.Point)Consume(TokenType.POINT, "Expected POINT as first parameter").Literal!;
        Expr p1 = ParseExpression();
        Consume(TokenType.COMMA, "Expected comma `,`");
        Expr p2 = ParseExpression();
        //p2 = (Element.Point)Consume(TokenType.POINT, "Expected POINT as second parameter").Literal!;
        Consume(TokenType.RIGHT_PAREN, "Expected `)`");


        Token id = Consume(TokenType.ID, "Expected identifier");

        //If the current token is an string then its a comment on the line.
        string comment = "";
        if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;

        return new Stmt.Lines(_line, _offset, id, p1, p2, new Element.String(comment));
    }
    private Stmt.Segment ParseSegmentStmt()
    {
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.SEGMENT, "Expected `segment` keyword");

        Consume(TokenType.LEFT_PAREN);
        // p1 = (Element.Point)Consume(TokenType.POINT, "Expected POINT as first parameter").Literal!;
        Expr p1 = ParseExpression();
        Consume(TokenType.COMMA, "Expected comma `,`");
        Expr p2 = ParseExpression();
        //p2 = (Element.Point)Consume(TokenType.POINT, "Expected POINT as second parameter").Literal!;
        Consume(TokenType.RIGHT_PAREN, "Expected `)`");


        Token id = Consume(TokenType.ID, "Expected identifier");

        //If the current token is an string then its a comment on the line.
        string comment = "";
        if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;

        return new Stmt.Segment(_line, _offset, id, p1, p2, new Element.String(comment));
    }
    private Stmt.Ray ParseRayStmt()
    {
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.RAY, "Expected `segment` keyword");

        Consume(TokenType.LEFT_PAREN);
        // p1 = (Element.Point)Consume(TokenType.POINT, "Expected POINT as first parameter").Literal!;
        Expr p1 = ParseExpression();
        Consume(TokenType.COMMA, "Expected comma `,`");
        Expr p2 = ParseExpression();
        //p2 = (Element.Point)Consume(TokenType.POINT, "Expected POINT as second parameter").Literal!;
        Consume(TokenType.RIGHT_PAREN, "Expected `)`");


        Token id = Consume(TokenType.ID, "Expected identifier");

        //If the current token is an string then its a comment on the line.
        string comment = "";
        if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;

        return new Stmt.Ray(_line, _offset, id, p1, p2, new Element.String(comment));
    }
    private Stmt.Declaration.Constant ParseConstantDeclaration()
    {
        Token id = Consume(TokenType.ID, "Expected identifier");
        Consume(TokenType.EQUAL, "Expected `=`");
        Expr expr = ParseExpression();
        ErrorIfEmpty(expr, id.Line, id.Offset, $"Assigned empty expression to constant `{id.Lexeme}`");//Rule 2
        return new Stmt.Declaration.Constant(id, expr);
    }
    private Stmt.Print ParsePrintStmt()
    {
        int _line = Peek.Line;
        int _offset = Peek.Offset;
        Consume(TokenType.PRINT);
        return new Stmt.Print(_line, _offset, ParseExpression());
    }
    private Stmt.Color ParseColorStmt()
    {
        int line = Peek.Line;
        int offset = Peek.Offset;

        //If the keyword `restore` is found the color doesnt matter
        if (Peek.Type == TokenType.RESTORE)
        {
            Consume(TokenType.RESTORE);
            return new Stmt.Color(line, offset, Color.BLACK, true);
        }

        Color color = Color.BLACK;
        Consume(TokenType.COLOR);
        //If the `color` keyword is found , a built-in color must be provided.
        switch (Peek.Type)
        {
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
                OnErrorFound(Peek.Line, Peek.Offset, $"Expected built-in color");
                break;
        }

        Advance();//Consume the token who holds the color.
        return new Stmt.Color(line, offset, color);
    }
    private Stmt.Eval ParseEvalStmt(){
        Consume(TokenType.EVAL);
        int line = Previous.Line;
        int offset = Previous.Offset;
        Expr expr = ParseExpression();
        return new Stmt.Eval(line,offset,expr);
    }
    #endregion Statement parsing

    #region Expression parsing
    private Expr ParseExpression()
    {
        switch (Peek.Type)
        {
            case TokenType.LET:
                return ParseLetInExpression();
            case TokenType.IF:
                return ParseConditionalExpression();
            default:
                return ParseOrExpression();
        }
    }
    private Expr ParseLetInExpression()
    {
        Consume(TokenType.LET);
        int line = Previous.Line;
        int offset = Previous.Offset;
        Stmt.StmtList stmts = ParseStmtList(TokenType.IN);
        Consume(TokenType.IN);
        Expr expr = ParseExpression();
        ErrorIfEmpty(expr, line, offset, "Expected non-empty expression after `in` keyword");
        return new Expr.LetIn(line, offset, stmts, expr);
    }
    private Expr ParseConditionalExpression()
    {
        Consume(TokenType.IF);
        int line = Previous.Line;
        int offset = Previous.Offset;
        Expr condition = ParseExpression();
        ErrorIfEmpty(condition, line, offset, "Expected non-empty expression for condition");
        Consume(TokenType.THEN, "Expected `then` keyword");
        Expr thenBranchExpr = ParseExpression();
        ErrorIfEmpty(thenBranchExpr, line, offset, "Expected non-empty expression after `then`");
        Consume(TokenType.ELSE, "Expected `else` keyword");
        Expr elseBranchExpr = ParseExpression();
        ErrorIfEmpty(elseBranchExpr, line, offset, "Expected non-empty expression after `else`");
        return new Expr.Conditional(line, offset, condition, thenBranchExpr, elseBranchExpr);
    }
    private Expr ParseOrExpression()
    {
        Func<Expr> parseHigherPrecedence = () => ParseAndExpression();
        return ParseAssociativeBinaryOperator(parseHigherPrecedence, parseHigherPrecedence, 0, TokenType.OR);
    }
    private Expr ParseAndExpression()
    {
        Func<Expr> parseHigherPrecedence = () => ParseEqualityExpression();
        return ParseAssociativeBinaryOperator(parseHigherPrecedence, parseHigherPrecedence, 0, TokenType.AND);
    }
    private Expr ParseEqualityExpression()
    {
        Func<Expr> parseHigherPrecedence = () => ParseComparisonExpression();
        return ParseAssociativeBinaryOperator(parseHigherPrecedence, parseHigherPrecedence, 1, TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL);
    }
    private Expr ParseComparisonExpression()
    {
        Func<Expr> parseHigherPrecedence = () => ParseTermExpression();
        return ParseAssociativeBinaryOperator(parseHigherPrecedence, parseHigherPrecedence, 1, TokenType.LESS, TokenType.LESS_EQUAL, TokenType.GREATER, TokenType.GREATER_EQUAL);
    }
    private Expr ParseTermExpression()
    {
        Func<Expr> parseHigherPrecedence = () => ParseFactorExpression();
        return ParseAssociativeBinaryOperator(parseHigherPrecedence, parseHigherPrecedence, 0, TokenType.PLUS, TokenType.MINUS);
    }
    private Expr ParseFactorExpression()
    {
        Func<Expr> parseHigherPrecedence = () => ParsePowerExpression();
        return ParseAssociativeBinaryOperator(parseHigherPrecedence, parseHigherPrecedence, 0, TokenType.STAR, TokenType.SLASH, TokenType.PERCENT);
    }
    private Expr ParsePowerExpression()
    {
        Func<Expr> parseLeft = () => ParseUnaryExpression();
        Func<Expr> parseRight = () => ParsePowerExpression();//Right associativity, use recursion.
        return ParseAssociativeBinaryOperator(parseLeft, parseRight, 0, TokenType.CARET);
    }
    private Expr ParseUnaryExpression()
    {
        Expr expr;
        switch (Peek.Type)
        {
            case TokenType.BANG:
                Advance();//Consume the operator
                expr = ParseUnaryExpression();
                ErrorIfEmpty(expr, Previous.Line, Previous.Offset, "Expected non-empty expression as operand");
                return new Expr.Unary.Not(Previous.Line, Previous.Offset, expr);
            case TokenType.MINUS:
                Advance();//Consume the operator
                expr = ParseUnaryExpression();
                ErrorIfEmpty(expr, Previous.Line, Previous.Offset, "Expected non-empty expression as operand");
                return new Expr.Unary.Minus(Previous.Line, Previous.Offset, expr);
            default:
                return ParseVariableExpression();
        }
    }
    private Expr ParseVariableExpression()
    {
        if (Match(TokenType.ID))
        {
            Token id = Previous;
            return new Expr.Variable(id);
        }
        return ParsePrimaryExpression();
    }
    private Expr ParsePrimaryExpression()
    {
        switch (Peek.Type)
        {
            case TokenType.NUMBER:
                return new Expr.Number(Peek.Line, Peek.Offset, (float)Advance().Literal!);
            case TokenType.STRING:
                return new Expr.String(Peek.Line, Peek.Offset, (string)Advance().Literal!);
            case TokenType.LEFT_PAREN://Grouping expressions
                Consume(TokenType.LEFT_PAREN);
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");
                return expr;
            default: return Expr.EMPTY;//The empty expression
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
    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (type == Peek.Type)
            {
                Advance();
                return true;
            }
        }
        return false;
    }
    ///<summary>If <c>expr</c> is the EMPTY expression its detected as an error.</summary>
    private void ErrorIfEmpty(Expr expr, int line, int offset, string message, bool enforceAbort = false)
    {
        if (expr == Expr.EMPTY) OnErrorFound(line, offset, message, enforceAbort);
    }
    ///<summary>Common behaviour for parsing associative binary operators. A negative loopLimit greater than zero limits the number of times the operator can be associated, applied contiguosly.</summary>
    private Expr ParseAssociativeBinaryOperator(Func<Expr> parseLeft, Func<Expr> parseRight, int loopLimit, params TokenType[] types)
    {
        Expr left = parseLeft();
        int loopCount = 0;
        Token firstOperation = Peek;
        while (Match(types))
        {
            //Rule #9
            if (loopLimit > 0 && loopCount == loopLimit) OnErrorFound(Previous.Line, Previous.Offset, $"Cannot use '{Previous.Lexeme}' after '{firstOperation.Lexeme}'. Consider using parenthesis and/or logical operators.");
            Token operation = Previous;
            ErrorIfEmpty(left, operation.Line, operation.Offset, "Expected non-empty expression as left operand");
            Expr right = parseRight();//If the operation is right associative then the parseRight method will recursively call this method somehow.
            ErrorIfEmpty(right, operation.Line, operation.Offset, "Expected non-empty expression as right operand");
            left = Expr.Binary.MakeBinaryExpr(left.Line, left.Offset, operation, left, right);
            ++loopCount;
        }
        return left;
    }
    #endregion
}