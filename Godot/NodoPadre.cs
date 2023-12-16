using Godot;
using System;
using System.IO;
using GSharpCompiler;
using System.Text;

public partial class NodoPadre : Control
{
    Imprimidor imprimidor;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        imprimidor = new Imprimidor();

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    class Imprimidor : System.IO.TextWriter
    {
        public override Encoding Encoding => throw new NotImplementedException();
        public TextEdit Consola {get;set;}
        public override void WriteLine(object? value)
        {
            
            Consola.InsertTextAtCaret(value.ToString()+"\n");

        }

    }

    public void ToDraw()
    {
        //IDrawable dibujo;
        var lienzocontrol = GetNode<Lienzocontrol>("Lienzocontrol");
        var lienzo = GetNode<Lienzo>("Lienzocontrol/ColorRect/Lienzo");
        string direction = "../Godot/Salvas/Data/data.txt";
        var consola = GetNode<TextEdit>("Consola");
        imprimidor.Consola = consola;
        //var numeros = GetNode<TextEdit>("NumerosDelCodigo");

        var nodolector = GetNode<CodeEdit>("Lector_de_Codigo");
        nodolector.SelectAll();
        string texto = nodolector.GetSelectedText();
        nodolector.Deselect();
        using (StreamWriter sw = File.CreateText(direction))
        {
            sw.WriteLine(texto);
        }
        var flags = new GSharpCompiler.Compiler.Flags();
        flags.MaxCoordinate = (int)Math.Min(lienzocontrol.Size.Y / 2, (lienzocontrol.Size.X) / 2);

        flags.OutputStream = imprimidor;
        var respuesta = GSharpCompiler.Compiler.CompileFromFile(direction, flags);

        lienzo.dibujos = respuesta.Elements;
        lienzo.QueueRedraw();

        //Parte de la consola
        //consola.Clear();
        int n = 0;
        foreach (GSharpCompiler.Error error in respuesta.Errors)
        {
            consola.InsertLineAt(n, $"{error.Message} at {error.Line},{error.Offset}");
            n++;
        }
        if (n != 0)
        {
            consola.InsertLineAt(n, "----------------------------------------------------------------");
        }
    }



    //Funcionalidad del boton save
    public void export(string name)
    {

        while (File.Exists($"../Godot/Salvas/{name}.txt"))
        {
            name = name + "(copy)";
        }

        string direction = $"../Godot/Salvas/{name}.txt";
        var nodolector = GetNode<TextEdit>("Lector_de_Codigo");
        nodolector.SelectAll();
        string texto = nodolector.GetSelectedText();
        nodolector.Deselect();
        using (StreamWriter sw = File.CreateText(direction))
        {
            sw.WriteLine(texto);
        }


    }



    //Funcionalidad del boton clear
    public void clear()
    {
        var nodolector = GetNode<CodeEdit>("Lector_de_Codigo");
        nodolector.Clear();
    }

    //Funcionalidad del boton clearconsole
    public void clearconsole()
    {
        var nodolector = GetNode<TextEdit>("Consola");
        nodolector.Clear();
    }

    // Transforma de un color de Walle a uno del motor grafico en este caso GODOT
    public Godot.Color EquivalentColor(GSharpCompiler.Color color)
    {
        if (color == GSharpCompiler.Color.BLACK)
        {
            return Godot.Colors.Black;

        }
        else if (color == GSharpCompiler.Color.BLUE)
        {
            return Godot.Colors.Blue;

        }
        else if (color == GSharpCompiler.Color.CYAN)
        {
            return Godot.Colors.Cyan;

        }
        else if (color == GSharpCompiler.Color.GRAY)
        {
            return Godot.Colors.Gray;

        }
        else if (color == GSharpCompiler.Color.GREEN)
        {
            return Godot.Colors.Green;

        }
        else if (color == GSharpCompiler.Color.MAGENTA)
        {
            return Godot.Colors.Magenta;

        }
        else if (color == GSharpCompiler.Color.RED)
        {
            return Godot.Colors.Red;

        }
        else if (color == GSharpCompiler.Color.WHITE)
        {
            return Godot.Colors.White;

        }
        else if (color == GSharpCompiler.Color.YELLOW)
        {
            return Godot.Colors.Yellow;
        }
        return Godot.Colors.AntiqueWhite;
    }
    public void _on_lienzocontrol_item_rect_changed()
    {
        var lienzo = GetNode<Lienzo>("Lienzocontrol/ColorRect/Lienzo");
        var lienzocontrol = GetNode<Lienzocontrol>("Lienzocontrol");
        Vector2 nuevocentro = new Vector2((lienzocontrol.Size.X) / 2, (lienzocontrol.Size.Y) / 2);
        lienzo.Position = (nuevocentro);
        ToDraw();
    }


}
