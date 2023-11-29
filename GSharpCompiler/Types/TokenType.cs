namespace GSharpCompiler;
///<summary>Represents the different kinds of tokens on G#.</summary>
enum TokenType{
    //Grouping symbols.
    LEFT_PAREN,RIGHT_PAREN,

    //Operators.
    EQUAL,BANG,MINUS,CARET,STAR,SLASH,PERCENT,PLUS,LESS,LESS_EQUAL,GREATER,GREATER_EQUAL,EQUAL_EQUAL,BANG_EQUAL,

    //If-then-else expression.
    IF,THEN,ELSE,AND,OR,

    //Let-In expression
    LET,IN,

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

    //The `LINES` keyword
    LINE,

    //Colors
    COLOR,COLOR_BLACK,COLOR_BLUE,COLOR_CYAN,COLOR_GRAY,COLOR_GREEN,
    COLOR_RED,COLOR_MAGENTA,COLOR_WHITE,COLOR_YELLOW,RESTORE,
    
    //Represents the end of file token.
    EOF
     
}