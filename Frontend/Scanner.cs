/*
The scanner receives a source code string and output a list of tokens.
*/

namespace Frontend;

public class Scanner
{
    private List<Token> tokens = new List<Token>();//The tokens
    private string source;//Source code
    private int current = 0;//Next unprocessed character
    private int start = 0;//First character of the actual token
    private int line = 1;//Current line
    private int offset = 0;//Current character since the begining of the line

    private static readonly Dictionary<string,TokenType> keywords = new Dictionary<string, TokenType>()
    {
        {"point",TokenType.POINT}
    };

    public Scanner(string? _source){
        if(_source == null)source = "";
        else source = _source;
    }

    public List<Token> Scan(){
        while(!IsAtEnd){
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(TokenType.EOF,"",null,line,source.Length));//Its elegant
        return new List<Token>(tokens);//Returns a copy of the token list.
    }

    private void ScanToken(){
        char c = Advance();
        switch(c){
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break; 
            case '/': if(Match('/')) ScanComment(); break;
            case ',': AddToken(TokenType.COMMA);  break;
            case ';': AddToken(TokenType.SEMICOLON);  break;

            //Ignore tabs and whitespaces
            case ' ': 
            case '\t': 
                break;

            case '\n': OnNewLineFound(); break;

            default:
                if(IsAlpha(c)) ScanIdentifier();
                break;
        }
    }
    //Scan a comment.
    private void ScanComment(){
        //Discard tokens until a new line is found. The backslashes are already consumed.
        while(!IsAtEnd && Peek() != '\n')Advance();
    }

    //Scan an identifier. Can be also a keyword.
    private void ScanIdentifier(){
        while(IsAlphaNumeric(Peek()))Advance();//Consume every posible character (Maximal Munch principle)

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

    //Aid in the creation of tokens.
    private void AddToken(TokenType type){
        AddToken(type,null);
    }
    //Create a token
    private void AddToken(TokenType type,object? literal){
        string lexeme = source.Substring(start,current - start);
        tokens.Add(new Token(type,lexeme,literal,line,ComputeOffset));
    }

    //Hit the end of the source code
    private bool IsAtEnd {get => current >= source.Length;}

    //Returns the character at current position and move current one position ahead. This is called consume the character.
    private char Advance(){
        ++current;
        return source[current - 1];
    }
    //Return the character at current position but do not move current. 
    private char Peek(){
        if(IsAtEnd)return '\0';//Guarantee that a call to this method does not throw an exception. 
        return source[current];
    }
    //Return the character one position ahead of current but do not move current. This is called lookahead.
    private char PeekNext(){
        if(current + 1 >= source.Length)return '\0';//Guarantee that a call to this method does not throw an exception.
        return source[current + 1];
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