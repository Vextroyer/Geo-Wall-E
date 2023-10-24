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

    public static void PrintAst(Program program){
        AstPrinter printer = new AstPrinter();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The program has {program.Stmts.Count} statements.");
        Console.ForegroundColor = ConsoleColor.White;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--START--");
        Console.ForegroundColor = ConsoleColor.White;

        foreach(Stmt stmt in program.Stmts){
            Console.WriteLine(printer.Print(stmt));
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--END--");
        Console.ForegroundColor = ConsoleColor.White;
    }
}