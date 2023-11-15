/*
Represents the different kinds of tokens on G#.
*/

namespace Frontend;

public enum TokenType{
    //Grouping symbols.
    LEFT_PAREN,RIGHT_PAREN,

    //Operators.
    EQUAL,

    //Represents a single line comment '//'.
    COMMENT,

    //Control symbols.
    COMMA, SEMICOLON,

    //Represents an identifier.
    ID,

    //Represents an string.
    STRING,

    //Represents number literals.
    NUMBER,
    
    //The 'point' keyword.
    POINT,

    //The `print` keyword
    PRINT,
    
    //Represents the end of file token.
    EOF
}