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
    public override void OnErrorFound(int line, int offset, string file, string message, bool enforceAbort = false)
    {
        base.OnErrorFound(line, offset, file, message, enforceAbort);
        //If the execution doesnt stop enter recovery mode.
        throw new RecoveryModeException();
    }
    public override void OnErrorFound(IErrorLocalizator error, string message, bool enforceAbort = false)
    {
        base.OnErrorFound(error, message, enforceAbort);
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
        Stmt.StmtList stmts = new Stmt.StmtList(Peek.Line, Peek.Offset, Peek.ExposeFile);
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
        if (Peek.Type != stopAtThisType) OnErrorFound(Previous, $"Unexpected EOF encountered, could not reach token of type {stopAtThisType}", true);
        return stmts;
    }
    private Stmt ParseStmt()
    {
        Stmt? aux = null;
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
            case TokenType.CIRCLE:
                aux = ParseCircleStmt();
                break;
            case TokenType.ARC:
                aux = ParseArcStmt();
                break;
            case TokenType.ID:
                aux = ParseDeclaration();
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
            case TokenType.IMPORT:
                aux = ParseImportStmt();
                break;
            default:
                OnErrorFound(Peek, "Not a statement");
                break;
        }
        Consume(TokenType.SEMICOLON, "Semicolon expected after statement");
        return aux!;
    }
    private Stmt.Draw ParseDrawStmt()
    {
        Token drawToken = Consume(TokenType.DRAW, "Expected `draw` keyword");
        Expr expr = ParseExpression();
        ErrorIfEmpty(expr, drawToken, "Expected non-empty expression after `draw`");

        return new Stmt.Draw(drawToken.Line, drawToken.Offset, drawToken.ExposeFile, expr);

    }
    private Stmt.Point ParsePointStmt()
    {
        Token pointToken = Consume(TokenType.POINT, "Expected `point` keyword");
        if (Peek.Type == TokenType.LEFT_PAREN)
        {
            Consume(TokenType.LEFT_PAREN);
            if (Peek.Type != TokenType.RIGHT_PAREN)
            {

                Expr x = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                Expr y = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");


                Token id = Consume(TokenType.ID, "Expected identifier");

                //If the current token is an string then its a comment on the point.
                string comment = "";
                if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;
                return new Stmt.Point(pointToken.Line, pointToken.Offset, pointToken.ExposeFile, id, x, y, new Element.String(comment));

            }
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        }

        Token id2 = Consume(TokenType.ID, "Expected identifier");

        return new Stmt.Point(pointToken.Line, pointToken.Offset, pointToken.ExposeFile, id2);
    }

    private Stmt.Lines ParseLinesStmt()
    {
        Token lineToken = Consume(TokenType.LINE, "Expected `line` keyword");

        if (Peek.Type == TokenType.LEFT_PAREN)
        {
            Consume(TokenType.LEFT_PAREN);
            if (Peek.Type != TokenType.RIGHT_PAREN)
            {
                Expr p1 = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                Expr p2 = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");


                Token id = Consume(TokenType.ID, "Expected identifier");
                string comment = "";
                if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;

                return new Stmt.Lines(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, id, p1, p2, new Element.String(comment));
            }
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        }

        Token id2 = Consume(TokenType.ID, "Expected identifier");

        return new Stmt.Lines(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, id2);
    }
    private Stmt.Segment ParseSegmentStmt()
    {
        Token lineToken = Consume(TokenType.SEGMENT, "Expected `segment` keyword");
        if (Peek.Type == TokenType.LEFT_PAREN)
        {
            Consume(TokenType.LEFT_PAREN);
            if (Peek.Type != TokenType.RIGHT_PAREN)
            {
                Expr p1 = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                Expr p2 = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");
                Token id = Consume(TokenType.ID, "Expected identifier");

                string comment = "";
                if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;

                return new Stmt.Segment(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, id, p1, p2, new Element.String(comment));
            }
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        }
        Token id2 = Consume(TokenType.ID, "Expected identifier");
        return new Stmt.Segment(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, id2);
    }
    private Stmt.Ray ParseRayStmt()
    {
        Token lineToken = Consume(TokenType.RAY, "Expected `ray` keyword");
        if (Peek.Type == TokenType.LEFT_PAREN)
        {
            Consume(TokenType.LEFT_PAREN);
            if (Peek.Type != TokenType.RIGHT_PAREN)
            {
                Expr p1 = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                Expr p2 = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");

                Token id = Consume(TokenType.ID, "Expected identifier");

                string comment = "";
                if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;

                return new Stmt.Ray(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, id, p1, p2, new Element.String(comment));
            }

            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        }
        Token id2 = Consume(TokenType.ID, "Expected identifier");
        return new Stmt.Ray(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, id2);

    }
    private Stmt.Circle ParseCircleStmt()
    {
        Token circleToken = Consume(TokenType.CIRCLE, "Expected `circle` keyword");
        if (Peek.Type == TokenType.LEFT_PAREN)
        {
            Consume(TokenType.LEFT_PAREN);
            if (Peek.Type != TokenType.RIGHT_PAREN)
            {
                Expr p1 = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                float radius = (float)Consume(TokenType.NUMBER, "Expected NUMBER as second parameter").Literal!;
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");

                Token id = Consume(TokenType.ID, "Expected identifier");
                string comment = "";
                if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;
                return new Stmt.Circle(circleToken.Line, circleToken.Offset, circleToken.ExposeFile, id, p1, new Element.Number(radius), new Element.String(comment));
            }
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        }
        Token id2 = Consume(TokenType.ID, "Expected identifier");

        return new Stmt.Circle(circleToken.Line, circleToken.Offset, circleToken.ExposeFile, id2);
    }
    private Stmt.Arc ParseArcStmt()
    {
        Token arcToken = Consume(TokenType.ARC, "Expected `arc` keyword");
        if (Peek.Type == TokenType.LEFT_PAREN)
        {
            Consume(TokenType.LEFT_PAREN);
            if (Peek.Type != TokenType.RIGHT_PAREN)
            {
                Expr p1 = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                Expr p2 = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                Expr p3 = ParseExpression();
                Consume(TokenType.COMMA, "Expected comma `,`");
                float radius = (float)Consume(TokenType.NUMBER, "Expected NUMBER as second parameter").Literal!;
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");

                Token id = Consume(TokenType.ID, "Expected identifier");
                string comment = "";
                if (Peek.Type == TokenType.STRING) comment = (string)Advance().Literal!;
                return new Stmt.Arc(arcToken.Line, arcToken.Offset, arcToken.ExposeFile, id, p1, p2, p3, new Element.Number(radius), new Element.String(comment));
            }
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");

        }
        Token id2 = Consume(TokenType.ID, "Expected identifier");
        return new Stmt.Arc(arcToken.Line, arcToken.Offset, arcToken.ExposeFile, id2);
    }
    ///<summary>Parse a declaration statement.</summary>
    private Stmt.Declaration ParseDeclaration()
    {
        //The actual token is an identifier.
        //The type of declaration depends on the next token
        switch (PeekNext.Type)
        {
            case TokenType.EQUAL:
                //Identifier followed by an equal sign means a constant declaration.
                return ParseConstantDeclaration();
            case TokenType.LEFT_PAREN:
                //Identifier followed by parentheses means a function declaration.
                return ParseFunctionDeclaration();
            default:
                OnErrorFound(Peek, "Identifier found on top level statement, but no declaration follows. If you intend to evaluate an expression use the `eval` keyword before the identifier.");
                break;
        }
        //Unreachable code.
        throw new Exception("Invalid execution path reached");
    }
    private Stmt.Declaration.Constant ParseConstantDeclaration()
    {
        Token id = Consume(TokenType.ID, "Expected identifier");
        Consume(TokenType.EQUAL, "Expected `=`");
        Expr expr = ParseExpression();
        ErrorIfEmpty(expr, id, $"Assigned empty expression to constant `{id.Lexeme}`");//Rule 2
        return new Stmt.Declaration.Constant(id, id.ExposeFile, expr);
    }
    private Stmt.Declaration.Function ParseFunctionDeclaration()
    {
        Token id = Consume(TokenType.ID, "Expected identifier on function declaration");
        Consume(TokenType.LEFT_PAREN);

        ///<summary>Helper method to parse the arguments of a function.</summary>
        List<Token> ParseFunctionArguments()
        {
            List<Token> arguments = new List<Token>();
            if (Peek.Type == TokenType.RIGHT_PAREN) return arguments;//A closing parenthesis means no arguments.

            do
            {
                Consume(TokenType.ID, "Identifier expected as argument");
                arguments.Add(Previous);
            } while (Match(TokenType.COMMA));
            return arguments;
        }

        List<Token> arguments = ParseFunctionArguments();
        Consume(TokenType.RIGHT_PAREN, "Expected `)` on function declaration");
        Consume(TokenType.EQUAL, "Expected `=` after function signature");
        Expr body = ParseExpression();
        ErrorIfEmpty(body, id, "Expected non-empty expression as function body");
        return new Stmt.Declaration.Function(id, id.ExposeFile, arguments, body);
    }
    private Stmt.Print ParsePrintStmt()
    {
        Token printToken = Consume(TokenType.PRINT);
        return new Stmt.Print(printToken.Line, printToken.Offset, printToken.ExposeFile, ParseExpression());
    }
    private Stmt.Color ParseColorStmt()
    {
        int line = Peek.Line;
        int offset = Peek.Offset;

        //If the keyword `restore` is found the color doesnt matter
        if (Peek.Type == TokenType.RESTORE)
        {
            Consume(TokenType.RESTORE);
            return new Stmt.Color(line, offset, Previous.ExposeFile, Color.BLACK, true);
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
                OnErrorFound(Peek, $"Expected built-in color");
                break;
        }

        Advance();//Consume the token who holds the color.
        return new Stmt.Color(line, offset, Previous.ExposeFile, color);
    }
    private Stmt.Eval ParseEvalStmt()
    {
        Token evalToken = Consume(TokenType.EVAL);
        Expr expr = ParseExpression();
        return new Stmt.Eval(evalToken.Line, evalToken.Offset, evalToken.ExposeFile, expr);
    }
    ///<summary>Auxiliary method used by the DependencyResolver to obtain the files to import from a given file.</summary>
    internal List<string> ParseImports()
    {
        List<string> imports = new List<string>();
        while (Match(TokenType.IMPORT))
        {
            Token file = Consume(TokenType.STRING, $"Expected STRING after `import` but {Peek.Type} found");
            imports.Add((string)file.Literal!);
            Consume(TokenType.SEMICOLON, "Expected `;` after `import` statement.");
        }
        //Detect imports which are not at the top.
        while (!IsAtEnd)
        {
            //These imports are not at the top.
            if (Peek.Type == TokenType.IMPORT)
            {
                try
                {
                    OnErrorFound(Peek, $"`import` must be placed before any other statements");
                }
                catch (RecoveryModeException)
                {

                }
            }
            Advance();
        }
        return imports;
    }
    ///<summary>Parse an import statement. Check that syntax is correct and ignore the statement because it has already been used by the DependencyResolver.</summary>
    public Stmt ParseImportStmt()
    {
        Consume(TokenType.IMPORT);
        Consume(TokenType.STRING, $"Expected STRING after `import` but {Peek.Type} found");
        return Stmt.EMPTY;
    }
    #endregion Statement parsing

    #region Expression parsing
    private Expr ParseExpression()
    {
        switch (Peek.Type)
        {
            case TokenType.ARC:
                return ParseArcExpression();
            case TokenType.CIRCLE:
                return ParseCircleExpression();
            case TokenType.LINE:
                return ParseLinesExpression();
            case TokenType.SEGMENT:
                return ParseSegmentExpression();
            case TokenType.RAY:
                return ParseRayExpression();
            case TokenType.POINT:
                return ParsePointExpression();
            case TokenType.LET:
                return ParseLetInExpression();
            case TokenType.IF:
                return ParseConditionalExpression();
            default:
                return ParseOrExpression();
        }
    }
    private Expr ParsePointExpression()
    {
        Token pointToken = Consume(TokenType.POINT);

        Consume(TokenType.LEFT_PAREN,"Expected `(` on call to `point`");
        if (Peek.Type != TokenType.RIGHT_PAREN)
        {
            Expr x = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            Expr y = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "Expected `)` on call to `point`");

            return new Expr.Point(pointToken.Line, pointToken.Offset, pointToken.ExposeFile, x, y);

        }
        Consume(TokenType.RIGHT_PAREN, "Expected `)`");

        return new Expr.Point(pointToken.Line, pointToken.Offset, pointToken.ExposeFile);

    }
    private Expr.Lines ParseLinesExpression()
    {
        Token lineToken = Consume(TokenType.LINE, "Expected `line` keyword");

        Consume(TokenType.LEFT_PAREN);
        if (Peek.Type != TokenType.RIGHT_PAREN)
        {
            Expr p1 = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            Expr p2 = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
            return new Expr.Lines(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, p1, p2);
        }

        Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        System.Console.WriteLine("RAMON");
        return new Expr.Lines(lineToken.Line, lineToken.Offset, lineToken.ExposeFile);
    }
    private Expr.Segment ParseSegmentExpression()
    {
        Token lineToken = Consume(TokenType.SEGMENT, "Expected `segment` keyword");

        Consume(TokenType.LEFT_PAREN);
        if (Peek.Type != TokenType.RIGHT_PAREN)
        {
            Expr p1 = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            Expr p2 = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");

            return new Expr.Segment(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, p1, p2);
        }
        Consume(TokenType.RIGHT_PAREN, "Expected `)`");

        return new Expr.Segment(lineToken.Line, lineToken.Offset, lineToken.ExposeFile);
    }
    private Expr.Ray ParseRayExpression()
    {
        Token lineToken = Consume(TokenType.RAY, "Expected `ray` keyword");

        Consume(TokenType.LEFT_PAREN);
        if (Peek.Type != TokenType.RIGHT_PAREN)
        {
            Expr p1 = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            Expr p2 = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");

            return new Expr.Ray(lineToken.Line, lineToken.Offset, lineToken.ExposeFile, p1, p2);
        }

        Consume(TokenType.RIGHT_PAREN, "Expected `)`");

        return new Expr.Ray(lineToken.Line, lineToken.Offset, lineToken.ExposeFile);
    }
    private Expr.Circle ParseCircleExpression()
    {
        Token circleToken = Consume(TokenType.CIRCLE, "Expected `circle` keyword");

        Consume(TokenType.LEFT_PAREN);
        if (Peek.Type != TokenType.RIGHT_PAREN)
        {
            Expr p1 = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            float radius = (float)Consume(TokenType.NUMBER, "Expected NUMBER as second parameter").Literal!;
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
            return new Expr.Circle(circleToken.Line, circleToken.Offset, circleToken.ExposeFile, p1, new Element.Number(radius));
        }
        Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        return new Expr.Circle(circleToken.Line, circleToken.Offset, circleToken.ExposeFile);
    }

    private Expr.Arc ParseArcExpression()
    {
        Token arcToken = Consume(TokenType.ARC, "Expected `arc` keyword");

        Consume(TokenType.LEFT_PAREN,"Expected `(` after call to `arc`");
        if (Peek.Type != TokenType.RIGHT_PAREN)
        {
            Expr p1 = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            Expr p2 = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            Expr p3 = ParseExpression();
            Consume(TokenType.COMMA, "Expected comma `,`");
            float radius = (float)Consume(TokenType.NUMBER, "Expected NUMBER as second parameter").Literal!;
            Consume(TokenType.RIGHT_PAREN, "Expected `)`");
            return new Expr.Arc(arcToken.Line, arcToken.Offset, arcToken.ExposeFile, p1, p2, p3, new Element.Number(radius));
        }
        Consume(TokenType.RIGHT_PAREN, "Expected `)`");
        return new Expr.Arc(arcToken.Line, arcToken.Offset, arcToken.ExposeFile);
    }
    private Expr ParseLetInExpression()
    {
        Token letToken = Consume(TokenType.LET);
        Stmt.StmtList stmts = ParseStmtList(TokenType.IN);
        Consume(TokenType.IN);
        Expr expr = ParseExpression();
        ErrorIfEmpty(expr, letToken, "Expected non-empty expression after `in` keyword");
        return new Expr.LetIn(letToken.Line, letToken.Offset, letToken.ExposeFile, stmts, expr);
    }
    private Expr ParseConditionalExpression()
    {
        Token ifToken = Consume(TokenType.IF);
        Expr condition = ParseExpression();
        ErrorIfEmpty(condition, ifToken, "Expected non-empty expression for condition");
        Consume(TokenType.THEN, "Expected `then` keyword");
        Expr thenBranchExpr = ParseExpression();
        ErrorIfEmpty(thenBranchExpr, ifToken, "Expected non-empty expression after `then`");
        Consume(TokenType.ELSE, "Expected `else` keyword");
        Expr elseBranchExpr = ParseExpression();
        ErrorIfEmpty(elseBranchExpr, ifToken, "Expected non-empty expression after `else`");
        return new Expr.Conditional(ifToken.Line, ifToken.Offset, ifToken.ExposeFile, condition, thenBranchExpr, elseBranchExpr);
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
                ErrorIfEmpty(expr, Previous, "Expected non-empty expression as operand");
                return new Expr.Unary.Not(Previous.Line, Previous.Offset, Previous.ExposeFile, expr);
            case TokenType.MINUS:
                Advance();//Consume the operator
                expr = ParseUnaryExpression();
                ErrorIfEmpty(expr, Previous, "Expected non-empty expression as operand");
                return new Expr.Unary.Minus(Previous.Line, Previous.Offset, Previous.ExposeFile, expr);
            default:
                return ParseVariableOrCallExpression();
        }
    }
    private Expr ParseVariableOrCallExpression()
    {
        //Parse a variable or a call
        switch(Peek.Type){
            case TokenType.MEASURE:
                return ParseMeasureExpr();
            case TokenType.ID:
                Token id = Advance();
                if(Match(TokenType.LEFT_PAREN)){
                    //Parse a call
                    List<Expr> parameters = ParseParameters();
                    Consume(TokenType.RIGHT_PAREN,$"Expected `)` after parameters on call to `{id.Lexeme}`");
                    return new Expr.Call(id,id.ExposeFile,parameters);
                }
                //Parse a variable
                return new Expr.Variable(id,id.ExposeFile);
            
            default : return ParsePrimaryExpression();
        }
        ///<summary>Parse a function call parameters.</summary>
        List<Expr> ParseParameters()
        {
            List<Expr> parameters = new List<Expr>();
            //A closing parenthesis means no parameters.
            if (Peek.Type == TokenType.RIGHT_PAREN) return parameters;
            do
            {
                parameters.Add(ParseExpression());
            } while (Match(TokenType.COMMA));
            return parameters;
        }
    }
    private Expr ParseMeasureExpr(){
        Token measureToken = Consume(TokenType.MEASURE);
        Consume(TokenType.LEFT_PAREN,$"Expected `(` after call to function `measure`");
        Expr firstPoint = ParseExpression();
        ErrorIfEmpty(firstPoint,Previous,"Expected non-empty expression as first parameter");
        Consume(TokenType.COMMA,"Expected `,` after parameter");
        Expr secondPoint = ParseExpression();
        ErrorIfEmpty(secondPoint,Previous,"Expectedd non-empty expression as second parameter.");
        Consume(TokenType.RIGHT_PAREN,"Expected `)` after parameters");
        return new Expr.Measure(measureToken,firstPoint,secondPoint);
    }
    private Expr ParsePrimaryExpression()
    {
        switch (Peek.Type)
        {
            case TokenType.NUMBER:
                return new Expr.Number(Peek.Line, Peek.Offset, Peek.ExposeFile, (float)Advance().Literal!);
            case TokenType.STRING:
                return new Expr.String(Peek.Line, Peek.Offset, Peek.ExposeFile, (string)Advance().Literal!);
            case TokenType.LEFT_PAREN://Grouping expressions
                Consume(TokenType.LEFT_PAREN);
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expected `)`");
                return expr;
            case TokenType.LEFT_BRACE: //Sequence
                return ParseSequenceExpr();
            default: return Expr.EMPTY;//The empty expression
        }
    }
    private Expr ParseSequenceExpr(){
        Token leftBraceToken = Consume(TokenType.LEFT_BRACE);
        List<Expr> expressions = new List<Expr>();//The expressions on the sequence.
        bool hasThreeDots = false;
        if(Peek.Type != TokenType.RIGHT_BRACE){
            if(Peek.Type == TokenType.THREE_DOTS)OnErrorFound(Peek,"Cannot start sequence declaration with `...`");
            Expr expr = ParseExpression();
            ErrorIfEmpty(expr,leftBraceToken,"Expected non-empty expression on sequence declaration");
            expressions.Add(expr);
            if(Match(TokenType.THREE_DOTS)){
                hasThreeDots = true;//Consme the three dots
                if(Match(TokenType.RIGHT_BRACE))return new Expr.Sequence(leftBraceToken.Line,leftBraceToken.Offset,leftBraceToken.ExposeFile,hasThreeDots,expressions);
                expr = ParseExpression();
                ErrorIfEmpty(expr,Peek,"Expected non-empty expression after `...`");
                expressions.Add(expr);
                Consume(TokenType.RIGHT_BRACE,"Expected `}` after sequence elements");
                return new Expr.Sequence(leftBraceToken.Line,leftBraceToken.Offset,leftBraceToken.ExposeFile,hasThreeDots,expressions);
            }
            if(Peek.Type!=TokenType.RIGHT_BRACE){
                Consume(TokenType.COMMA,"Expected `,` after expression");
                //Since now encountering any three dots is an error
                do{
                    if(Peek.Type == TokenType.THREE_DOTS)OnErrorFound(Peek,"Three dots can only be used on sequence declarations of the form {Expr ...} or {Expr ... Expr}");
                    expr = ParseExpression();
                    ErrorIfEmpty(expr,Peek,"Expected non-empty expression on sequence declaration");
                    expressions.Add(expr);
                }while(Match(TokenType.COMMA));
            }
        }
        Consume(TokenType.RIGHT_BRACE,"Expected `}` after sequence elements");
        return new Expr.Sequence(leftBraceToken.Line,leftBraceToken.Offset,leftBraceToken.ExposeFile,hasThreeDots,expressions);
    }
    #endregion Expression parsing

    #region  Auxiliary Methods

    private bool IsAtEnd { get => Peek.Type == TokenType.EOF; }

    //Return the current token without moving current,which without consuming its.
    private Token Peek { get => tokens[current]; }
    ///<summary>The token after `current` token. If there is no more tokens after `current` returns the last token on `tokens`.</summary>
    private Token PeekNext
    {
        get
        {
            if (current + 1 < tokens.Count) return tokens[current + 1];
            return tokens[tokens.Count - 1];
        }
    }
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
        OnErrorFound(Previous.Line, Previous.Offset + Previous.Lexeme.Length, Previous.File, message);
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
    private void ErrorIfEmpty(Expr expr, IErrorLocalizator error, string message, bool enforceAbort = false)
    {
        if (expr == Expr.EMPTY) OnErrorFound(error, message, enforceAbort);
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
            if (loopLimit > 0 && loopCount == loopLimit) OnErrorFound(Previous, $"Cannot use '{Previous.Lexeme}' after '{firstOperation.Lexeme}'. Consider using parenthesis and/or logical operators.");
            Token operation = Previous;
            ErrorIfEmpty(left, operation, "Expected non-empty expression as left operand");
            Expr right = parseRight();//If the operation is right associative then the parseRight method will recursively call this method somehow.
            ErrorIfEmpty(right, operation, "Expected non-empty expression as right operand");
            left = Expr.Binary.MakeBinaryExpr(left.Line, left.Offset, left.ExposeFile, operation, left, right);
            ++loopCount;
        }
        return left;
    }
    #endregion
}