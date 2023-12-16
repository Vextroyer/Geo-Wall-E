using Godot;
using System;

public partial class Save : Button
{
	public string name="";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}
	void _pressed()
	{
		var NameMenu=GetNode<Panel>("Menu_de_Nombre");
		NameMenu.Visible=true;
		//while(NameMenu.Visible==true)
		//{
		//	GD.Print("Esperando");
		//}
		

	}
	public void callexport()
	{
		var nodocontrol = GetNode<NodoPadre>("..");
		nodocontrol.export(name);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
