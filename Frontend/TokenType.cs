/*
Represents the different kinds of tokens on G#.
*/

namespace Frontend;

enum TokenType{
    //Grouping symbols.
    LEFT_PAREN,RIGHT_PAREN,

    //Operators.
    EQUAL,NOT,MINUS,CARET,STAR,SLASH,PERCENT,

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
  
     //The `draw` keyword
    DRAW,

    //The `print` keyword
    PRINT,

    //Colors
    COLOR,COLOR_BLACK,COLOR_BLUE,COLOR_CYAN,COLOR_GRAY,COLOR_GREEN,
    COLOR_RED,COLOR_MAGENTA,COLOR_WHITE,COLOR_YELLOW,RESTORE,
    
    //Represents the end of file token.
    EOF
     
}