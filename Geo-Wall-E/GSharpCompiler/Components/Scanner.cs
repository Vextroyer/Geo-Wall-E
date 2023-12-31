/*
The scanner receives a source code string and output a list of tokens.
*/

namespace GSharpCompiler;

class Scanner : GSharpCompilerComponent
{
    private List<Token> tokens = new List<Token>();//The tokens
    private string source;//Source code
    private int current = 0;//Next unprocessed character
    private int start = 0;//First character of the actual token
    private int line = 1;//Current line
    private int offset = 0;//Current character since the begining of the line
    ///<summary>The name of the file being scanned.</summary>
    private char[] fileName;

    private static readonly Dictionary<string,TokenType> keywords = new Dictionary<string, TokenType>()
    {
        {"point",TokenType.POINT},
        {"draw",TokenType.DRAW},
        {"print",TokenType.PRINT},
        {"if",TokenType.IF},
        {"then",TokenType.THEN},
        {"else",TokenType.ELSE},
        {"and",TokenType.AND},
        {"or",TokenType.OR},
        {"let",TokenType.LET},
        {"in",TokenType.IN},
        {"line",TokenType.LINE},
        {"segment",TokenType.SEGMENT},
        {"ray",TokenType.RAY},
        {"circle",TokenType.CIRCLE},
        {"arc",TokenType.ARC},
        {"eval",TokenType.EVAL},
        {"import",TokenType.IMPORT},
        {"measure",TokenType.MEASURE},
        {"intersect",TokenType.INTERSECT},
        {"count",TokenType.COUNT},
        {"randoms",TokenType.RANDOMS},
        {"samples",TokenType.SAMPLES},
        //Colors
        {"color",TokenType.COLOR},
        {"black",TokenType.COLOR_BLACK},
        {"blue",TokenType.COLOR_BLUE},
        {"cyan",TokenType.COLOR_CYAN},
        {"gray",TokenType.COLOR_GRAY},
        {"green",TokenType.COLOR_GREEN},
        {"magenta",TokenType.COLOR_MAGENTA},
        {"red",TokenType.COLOR_RED},
        {"white",TokenType.COLOR_WHITE},
        {"yellow",TokenType.COLOR_YELLOW},
        {"restore",TokenType.RESTORE}
  
    };
    public Scanner(string? _source,string _fileName,int maxErrorCount,ICollection<GSharpCompiler.Error> errors):base(maxErrorCount,errors){
        if(_source == null)source = "";
        else source = _source;
        fileName = _fileName.ToCharArray();
    }
    ///<summary>Abort by trowhing a <c>ScannerException</c> .</summary>
    public override void Abort(){
        throw new ScannerException();
    }
    
    public List<Token> Scan(){
        while(!IsAtEnd){
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(TokenType.EOF,"",null,line,source.Length,fileName));//Its elegant
        return new List<Token>(tokens);//Returns a copy of the token list.
    }

    private void ScanToken(){
        char c = Advance();
        switch(c){
            case '"': ScanString(); break;
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break; 
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case '/': 
                if(Match('/')) ScanComment();
                else AddToken(TokenType.SLASH); 
                break;
            case ',': AddToken(TokenType.COMMA);  break;
            case ';': AddToken(TokenType.SEMICOLON);  break;
            case '=': 
                if(Match('='))AddToken(TokenType.EQUAL_EQUAL);
                else AddToken(TokenType.EQUAL); 
                break;
            case '!':
                if(Match('=')) AddToken(TokenType.BANG_EQUAL);
                else AddToken(TokenType.BANG); 
                break;
            case '<':
                if(Match('=')) AddToken(TokenType.LESS_EQUAL);
                else AddToken(TokenType.LESS);
                break;
            case '>':
                if(Match('=')) AddToken(TokenType.GREATER_EQUAL);
                else AddToken(TokenType.GREATER);
                break;
            case '&': AddToken(TokenType.AND); break;
            case '|': AddToken(TokenType.OR); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '^': AddToken(TokenType.CARET); break;
            case '%': AddToken(TokenType.PERCENT); break;
            case '*': AddToken(TokenType.STAR); break;
            case '+': AddToken(TokenType.PLUS); break;

            //Ignore tabs and whitespaces
            case ' ': 
            case '\t': 
                break;

            case '\n': OnNewLineFound(); break;
            case '\r': break;

            default:
                if(IsDigit(c)){
                    ScanNumber();
                    break;
                }
                else if(IsAlpha(c)) {
                    ScanIdentifier();
                    break;
                }
                else if(c == '.'){
                    if(!TryParseThreeDots())OnErrorFound(line,ComputeOffset,new string(fileName),"Expected digit before `.`");
                    break;
                }
                OnErrorFound(line,ComputeOffset,new string(fileName),"Unrecognized character");
                break;
        }
    }
    private bool TryParseThreeDots(){
        if(Peek == '.' && PeekNext == '.'){
            Advance();Advance();//Consume the three dots
            AddToken(TokenType.THREE_DOTS);
            return true;
        }
        return false;
    }
    //Scan a string literal, the previous character was a quote '"'
    //Consume characters until it hits another quote. If the end is reached then a quote is missing.
    private void ScanString(){
        //Stop at end or if a quote is found
        
        int openingQuoteLine = line;//Store the position on the code of the opening quote for error reporting
        int openingQuoteOffset = ComputeOffset;

        while(!IsAtEnd && Peek != '"'){ 
            char c = Advance();
            if(c == '\n')OnNewLineFound();//This is for supporting multi-line strings
        }

        if(IsAtEnd) OnErrorFound(openingQuoteLine,openingQuoteOffset,new string(fileName),"Opening quote whitout enclosing quote found",true);
        Advance();//Consume the closing quote

        string value = source.Substring(start + 1,current - start - 2);//The string content without the enclosing quotes

        AddToken(TokenType.STRING,value);
    }

    //Scan a comment.
    private void ScanComment(){
        //Discard tokens until a new line is found. The backslashes are already consumed.
        while(!IsAtEnd && Peek != '\n')Advance();
    }

    //Scan an identifier. Can be also a keyword.
    private void ScanIdentifier(){
        while(IsAlphaNumeric(Peek))Advance();//Consume every posible character (Maximal Munch principle)

        string lexeme = source.Substring(start,current - start);
        TokenType type;
        try{
            //Is a keyword identifier
            type = keywords[lexeme];
        }catch(KeyNotFoundException){
            type = TokenType.ID;
        }
        AddToken(type);
    }

    //Scan a number literal
    private void ScanNumber(){
        while(IsDigit(Peek))Advance();//Consume the leading digits
        
        //If there exist a dot after a digit.
        if(Peek == '.'){
            Advance();//Consume the '.'
            
            //If there is no digit after the dot then its an error
            if(!IsDigit(Peek))OnErrorFound(line,ComputeOffset,new string(fileName),"Expected digit after `.`");

            while(IsDigit(Peek))Advance();//Consume the trailing digits
        }

        //If there exist an alphanumeric character after a digit, then this is misstyped identifier.
        if(IsAlpha(Peek)) OnErrorFound(line,ComputeOffset,new string(fileName),"Identifiers can't start with numbers");

        AddToken(TokenType.NUMBER,float.Parse(source.Substring(start,current - start)));
    }

    //Aid in the creation of tokens.
    private void AddToken(TokenType type){
        AddToken(type,null);
    }
    //Create a token
    private void AddToken(TokenType type,object? literal){
        string lexeme = source.Substring(start,current - start);
        tokens.Add(new Token(type,lexeme,literal,line,ComputeOffset,fileName));
    }

    //Hit the end of the source code
    private bool IsAtEnd {get => current >= source.Length;}

    //Returns the character at current position and move current one position ahead. This is called consume the character.
    private char Advance(){
        ++current;
        return source[current - 1];
    }
    //Return the character at current position but do not move current. 
    private char Peek{
        get{
            if(IsAtEnd)return '\0';//Guarantee that a call to this method does not throw an exception. 
            return source[current];
        }
        
    }
    //Return the character one position ahead of current but do not move current. This is called lookahead.
    private char PeekNext{
        get{
            if(current + 1 >= source.Length)return '\0';//Guarantee that a call to this method does not throw an exception.
            return source[current + 1];
        }
    }
    //Conditional advance, si el caracter actual coincide con el dado retorna verdadero y consume el caracter actual,
    //si no retorna falso y se mantiene el caracter actual
    private bool Match(char expected){
        if(IsAtEnd || source[current] != expected)return false;

        ++current;
        return true;
    }
    //Return true if the character is a decimal digit [0,9], otherwhise return false.
    private bool IsDigit(char c){
        if('0' <= c && c <= '9')return true;
        return false;
    }
    //Return true if the character is a letter or an underscore '_'
    //
    private bool IsAlpha(char c){
        return ('a' <= c && c <= 'z') ||
               ('A' <= c && c <= 'Z') ||
               (c == '_');
    }
    //Return true if the character is a letter, a digit or an underscore.
    private bool IsAlphaNumeric(char c){
        return IsDigit(c) || IsAlpha(c);
    }

    //If a new line is found update the line counter and restart the offset
    private void OnNewLineFound(){
        ++line;
        offset = current;
    }
    //Compute the offset of the actual character on the actual line
    private int ComputeOffset {get => start - offset;}
}