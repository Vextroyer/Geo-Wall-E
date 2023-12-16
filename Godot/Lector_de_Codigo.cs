using Godot;
using System;

public partial class Lector_de_Codigo : CodeEdit
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CodeHighlighter highlighter = new CodeHighlighter();

        highlighter.NumberColor = Colors.LightBlue;
        //highlighter.FunctionColor= Colors.Blue;
        highlighter.SymbolColor = Colors.Orange;

        //Funciones Walle
        highlighter.AddKeywordColor("draw", Colors.IndianRed);
        highlighter.AddKeywordColor("color", Colors.IndianRed);
        highlighter.AddKeywordColor("intersect", Colors.IndianRed);
        highlighter.AddKeywordColor("measure", Colors.IndianRed);
        highlighter.AddKeywordColor("count", Colors.IndianRed);
        highlighter.AddKeywordColor("samples", Colors.IndianRed);
        highlighter.AddKeywordColor("randoms", Colors.IndianRed);
        highlighter.AddKeywordColor("import", Colors.IndianRed);
        highlighter.AddKeywordColor("let", Colors.IndianRed);
        highlighter.AddKeywordColor("in", Colors.IndianRed);
        highlighter.AddKeywordColor("call", Colors.IndianRed);
        highlighter.AddKeywordColor("intersect", Colors.IndianRed);
        highlighter.AddKeywordColor("randoms", Colors.IndianRed);

        //IDRAWEABLE
        highlighter.AddKeywordColor("point", Colors.SeaGreen);
        highlighter.AddKeywordColor("circle", Colors.SeaGreen);
        highlighter.AddKeywordColor("line", Colors.SeaGreen);
        highlighter.AddKeywordColor("segment", Colors.SeaGreen);
        highlighter.AddKeywordColor("ray", Colors.SeaGreen);
        highlighter.AddKeywordColor("arc", Colors.SeaGreen);
        highlighter.AddKeywordColor("line", Colors.SeaGreen);


        //Colores
        highlighter.AddKeywordColor("green", Colors.Green);
        highlighter.AddKeywordColor("blue", Colors.Blue);
        highlighter.AddKeywordColor("red", Colors.Red);
        highlighter.AddKeywordColor("yellow", Colors.Yellow);
        highlighter.AddKeywordColor("magenta", Colors.Magenta);
        highlighter.AddKeywordColor("black", Colors.Black);
        highlighter.AddKeywordColor("cyan", Colors.Cyan);
        highlighter.AddKeywordColor("gray", Colors.Gray);
        highlighter.AddKeywordColor("white", Colors.White);

        this.SyntaxHighlighter = highlighter;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

}
