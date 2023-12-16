namespace GSharpCompiler;
///<summary>Represents the different kinds of tokens on G#.</summary>
enum TokenType{
    //Grouping symbols.
    LEFT_PAREN,RIGHT_PAREN,

    //Operators.
    EQUAL,BANG,MINUS,CARET,STAR,SLASH,PERCENT,PLUS,LESS,LESS_EQUAL,GREATER,GREATER_EQUAL,EQUAL_EQUAL,BANG_EQUAL,

    //Sequences
    LEFT_BRACE,RIGHT_BRACE,THREE_DOTS,

    //If-then-else expression.
    IF,THEN,ELSE,AND,OR,

    //Let-In expression
    LET,IN,

    //Control symbols.
    COMMA, SEMICOLON, IMPORT,

    //Represents an identifier.
    ID,

    //The `eval` keyword
    EVAL,

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

    //The `LINE` keyword
    LINE,

    //The `SEGMENT` keyword
    SEGMENT,

    //The `RAY` keyword
    RAY,

    //The `CIRCLE` keyword
    CIRCLE,
    //The `ARC` keyword
    ARC,
    //Some Keywords
    MEASURE,COUNT,RANDOMS,SAMPLES,INTERSECT,
    //Colors
    COLOR,COLOR_BLACK,COLOR_BLUE,COLOR_CYAN,COLOR_GRAY,COLOR_GREEN,
    COLOR_RED,COLOR_MAGENTA,COLOR_WHITE,COLOR_YELLOW,RESTORE,
    
    //Represents the end of file token.
    EOF
     
}