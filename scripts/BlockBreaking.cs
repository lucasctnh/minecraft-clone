using Godot;
using System;

public partial class BlockBreaking : Node3D
{
	[Export] private AnimatedSprite3D[] animatedSprite3Ds;

	public void ChangeFrame(int frame)
	{
		foreach (var animatedSprite3D in animatedSprite3Ds)
			animatedSprite3D.Frame = frame;
	}
}
