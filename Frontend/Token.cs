/*
Represents a token.
*/

namespace Frontend;

public class Token{
    public TokenType Type {get; private set;}//The type of the token.
    public string? Lexeme {get; private set;}//The piece of code who generated this token.
    public int Line {get; private set;}//The line on the code where the token was produced.
    public int Offset{get; private set;}//The amount of characters from the start of the line to the first character of the token.
    public object? Literal {get; private set;}//The literal value of the token

    public Token(TokenType type,string lexeme,object? literal,int line,int offset){
        this.Type = type;
        this.Lexeme = lexeme;
        this.Literal = literal;
        this.Line = line;
        this.Offset = offset;
    }

    public override string ToString()
    {
        return $"{Type} `{Lexeme}` {Literal} L:{Line} O:{Offset}";
    }
}