/*
Several utility methods and features.
*/

namespace Frontend;

public static class Utils{
    public static void PrintTokens(List<Token> tokens){
        Console.WriteLine("{");
        foreach(Token t in tokens)Console.WriteLine("\t" + t);
        Console.WriteLine("}");
    }
}