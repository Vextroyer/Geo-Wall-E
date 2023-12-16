using Godot;
using System;

public partial class Menu_de_Nombre : Panel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void accept()
	{
		var NameMenu=GetNode<TextEdit>("NombreDeArchivo");
		NameMenu.SelectAll();
		string texto=NameMenu.GetSelectedText();
		var save=GetNode<Save>("..");
		save.name=texto;
		NameMenu.Clear();
		Visible=false;
		save.callexport();



	}

	public void cancel()
	{
		var NameMenu=GetNode<TextEdit>("NombreDeArchivo");
		NameMenu.Clear();
		string texto=NameMenu.GetSelectedText();
		Visible=false;
	}
}
