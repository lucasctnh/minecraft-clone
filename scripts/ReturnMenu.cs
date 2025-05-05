using Godot;
using System;

public partial class ReturnMenu : Node3D
{
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("esc_menu"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
		}
	}
}
