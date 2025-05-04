using Godot;
using System;

public partial class CameraPivot : Node3D
{
	[Export] public float RotationSpeed = 8f;

	public override void _Process(double delta)
	{
		RotateY(RotationSpeed * (float)delta);
	}
}
