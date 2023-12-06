namespace GSharpCompiler;

///<summary>A token encapsulate every lexeme on the source code an give it a type and optionally a value.</summary>
class Token{
    ///<summary>The type of the token.</summary>
    public TokenType Type {get; private set;}//The type of the token.
    ///<summary>The source code string from which this token was created.</summary>
    public string Lexeme {get; private set;}//The piece of code who generated this token.
    ///<summary>The line where this token was found.</summary>
    public int Line {get; private set;}//The line on the code where the token was produced.
    ///<summary>The column where this token was found.</summary>
    public int Offset{get; private set;}//The amount of characters from the start of the line to the first character of the token.
    ///<summary>The value of this token. <example>Lexeme "2.25" has literal value of 2.25.</example></summary>
    public object? Literal {get; private set;}//The literal value of the token
    public string File {get => new string(file);}
    ///<summary>The name of the file which contains the source code.</summary>
    private char[] file;
    public Token(TokenType type,string lexeme,object? literal,int line,int offset,char[] fileName){
        this.Type = type;
        this.Lexeme = lexeme;
        this.Literal = literal;
        this.Line = line;
        this.Offset = offset;
        file = fileName;
    }

    public override string ToString()
    {
        return $"{Type} `{Lexeme}` {Literal} L:{Line} O:{Offset}";
    }
}