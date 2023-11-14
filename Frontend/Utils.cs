/*
Several utility methods and features.
*/

namespace Frontend;

public static class Utils{
    //Used for printing the output of the Scanner
    public static void PrintTokens(List<Token> tokens){
        Console.WriteLine("{");
        foreach(Token t in tokens)Console.WriteLine("\t" + t);
        Console.WriteLine("}");
    }
    //Used for printing the output of the Parser
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

    //Random point generation
    private static Random random = new Random();//Stores a random for future usage.
    private static int MaxPointCoordinate = 1000;//Represents the maximum absolute value of point coordinates
    public static (float,float) RandomPoint(){
        return (RandomCoordinate(),RandomCoordinate());
    }
    //Generate an integer on the range [-MaxPointCoordinate,MaxPointCoordinate]
    public static int RandomCoordinate(){
        return (random.Next() % (2 * MaxPointCoordinate + 1)) - MaxPointCoordinate;
    }
}