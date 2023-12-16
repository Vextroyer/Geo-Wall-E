using Godot;
using System;
using GSharpCompiler;
using System.Collections.Generic;

public partial class Lienzo : Node2D
{
    public List<IDrawable> dibujos { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {


    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    public override void _Draw()
    {//item rect changed
        float GrosorDePuntos = 4;
        float delta = 1000;
       

        var default_font = ThemeDB.FallbackFont;
        var default_font_size = ThemeDB.FallbackFontSize;

        var menu = GetNode<NodoPadre>("../../..");
        if (dibujos == null) return;


        foreach (IDrawable dibujo in dibujos)
        {
            if (dibujo.Type == ElementType.POINT)
            {
                Vector2 punto = new Vector2((float)(((Element.Point)dibujo).x.Value), -(float)(((Element.Point)dibujo).y.Value));
                DrawCircle(punto, GrosorDePuntos, menu.EquivalentColor(dibujo.Color));
                DrawString(default_font, punto, $"{((Element.Point)dibujo).name}  {((Element.Point)dibujo).Comment}", HorizontalAlignment.Left, -1, default_font_size, menu.EquivalentColor(dibujo.Color));
            }
            else if (dibujo.Type == ElementType.LINE)
            {
                Vector2 punto1 = new Vector2((float)(((Element.Lines)dibujo).p1.x.Value), -(float)(((Element.Lines)dibujo).p1.y.Value));
                Vector2 punto2 = new Vector2((float)(((Element.Lines)dibujo).p2.x.Value), -(float)(((Element.Lines)dibujo).p2.y.Value));
                //probablemente provisional
                DrawString(default_font, punto1, $"{((Element.Lines)dibujo).name}  {((Element.Lines)dibujo).Comment}", HorizontalAlignment.Left, -1, default_font_size, menu.EquivalentColor(dibujo.Color));

                var newvector = new Vector2(punto2.X - punto1.X, punto2.Y - punto1.Y);
                punto1 = new Vector2(punto2.X + delta * newvector.X, punto2.Y + delta * newvector.Y);
                punto2 = new Vector2(punto2.X + (-delta) * newvector.X, punto2.Y + (-delta) * newvector.Y);

                DrawLine(punto1, punto2, menu.EquivalentColor(dibujo.Color), 5f);


            }
            else if (dibujo.Type == ElementType.SEGMENT)
            {
                Vector2 punto1 = new Vector2((float)(((Element.Segment)dibujo).p1.x.Value), -(float)(((Element.Segment)dibujo).p1.y.Value));
                Vector2 punto2 = new Vector2((float)(((Element.Segment)dibujo).p2.x.Value), -(float)(((Element.Segment)dibujo).p2.y.Value));

                DrawLine(punto1, punto2, menu.EquivalentColor(dibujo.Color), 5f);
                DrawString(default_font, punto1, $"{((Element.Segment)dibujo).name}  {((Element.Segment)dibujo).Comment}", HorizontalAlignment.Left, -1, default_font_size, menu.EquivalentColor(dibujo.Color));

            }
            else if (dibujo.Type == ElementType.RAY)
            {
                Vector2 punto1 = new Vector2((float)(((Element.Ray)dibujo).p1.x.Value), -(float)(((Element.Ray)dibujo).p1.y.Value));
                Vector2 punto2 = new Vector2((float)(((Element.Ray)dibujo).p2.x.Value), -(float)(((Element.Ray)dibujo).p2.y.Value));


                var newvector = new Vector2(punto2.X - punto1.X, punto2.Y - punto1.Y);
                punto2 = new Vector2(punto2.X + (delta) * newvector.X, punto2.Y + (delta) * newvector.Y);

                DrawLine(punto1, punto2, menu.EquivalentColor(dibujo.Color), 5f);
                DrawString(default_font, punto1, $"{((Element.Ray)dibujo).name}  {((Element.Ray)dibujo).Comment}", HorizontalAlignment.Left, -1, default_font_size, menu.EquivalentColor(dibujo.Color));
            }
            else if (dibujo.Type == ElementType.CIRCLE)
            {
                Vector2 punto1 = new Vector2((float)(((Element.Circle)dibujo).p1.x.Value), -(float)(((Element.Circle)dibujo).p1.y.Value));

                DrawArc(punto1, (float)(((Element.Circle)dibujo).radius.Value), 0, (float)Math.PI * 2, 64, menu.EquivalentColor(dibujo.Color), 5.0f);
                DrawString(default_font, punto1, $"{((Element.Circle)dibujo).name}  {((Element.Circle)dibujo).Comment}", HorizontalAlignment.Left, -1, default_font_size, menu.EquivalentColor(dibujo.Color));
            }
            else if (dibujo.Type == ElementType.ARC)
            {
                Vector2 punto1 = new Vector2((float)(((Element.Arc)dibujo).p1.x.Value), -(float)(((Element.Arc)dibujo).p1.y.Value));
                Vector2 punto2 = new Vector2((float)(((Element.Arc)dibujo).p2.x.Value), -(float)(((Element.Arc)dibujo).p2.y.Value));
                Vector2 punto3 = new Vector2((float)(((Element.Arc)dibujo).p3.x.Value), -(float)(((Element.Arc)dibujo).p3.y.Value));

                float angulo1 = (float)(3 * (Math.PI / 2));
                float angulo2 = (float)(3 * (Math.PI / 2));

                if ((punto2.X - punto1.X) != 0)
                {
                    double pendiente1 = (double)((punto2.Y - punto1.Y) / (punto2.X - punto1.X));// division por cero
                    angulo1 = (float)Math.Atan((pendiente1 - 0) / (1 - pendiente1 * 0));
                }
                if ((punto3.X - punto1.X) != 0)
                {
                    double pendiente2 = (double)((punto3.Y - punto1.Y) / (punto3.X - punto1.X));
                    angulo2 = (float)Math.Atan((pendiente2 - 0) / (1 - pendiente2 * 0));
                }

                if (punto1.X > punto2.X)
                {
                    angulo1 = angulo1 + (float)((3 / 2) * Math.PI);
                }

                if (punto1.X > punto3.X)
                {
                    angulo2 = angulo2 + (float)((3 / 2) * Math.PI);

                }

                if (angulo1 > angulo2)
                {
                    DrawArc(punto1, (float)(((Element.Arc)dibujo).radius.Value), angulo1, angulo2, 64, menu.EquivalentColor(dibujo.Color), 5.0f);
                    DrawString(default_font, punto1, $"{((Element.Arc)dibujo).name}  {((Element.Arc)dibujo).Comment}", HorizontalAlignment.Left, -1, default_font_size, menu.EquivalentColor(dibujo.Color));
                }
                else
                {
                    angulo2 = (float)(angulo2 - 2 * Math.PI);
                    DrawArc(punto1, (float)(((Element.Arc)dibujo).radius.Value), angulo1, angulo2, 64, menu.EquivalentColor(dibujo.Color), 5.0f);
                    DrawString(default_font, punto1, $"{((Element.Arc)dibujo).name}  {((Element.Arc)dibujo).Comment}", HorizontalAlignment.Left, -1, default_font_size, menu.EquivalentColor(dibujo.Color));
                }
            }
        }
    }
    
    public void _on_color_rect_item_rect_changed()
    {

    }

}
