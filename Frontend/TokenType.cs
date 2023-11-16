/*
Represents the different kinds of tokens on G#.
*/

namespace Frontend;

public enum TokenType{
    LEFT_PAREN,RIGHT_PAREN,
    COMMENT,//Represents a single line comment '//'
    COMMA,
    SEMICOLON,//Represents a semicolon ';'
    ID,//Represents an identifier.
    STRING,//Represents an string
    NUMBER,//Represents number literals.
    POINT,//The 'point' keyword
    EOF,//Represents the end of file token.
    DRAW
}