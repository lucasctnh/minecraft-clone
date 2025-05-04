using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Signal]
	public delegate void BlockSelectedEventHandler(int hotbarIndex);

	[Export] public float Speed = 8f;
	[Export] public float JumpVelocity = 10f;
	[Export] public float Sensitivity = 0.002f;

	[Export] private Camera3D camera3D;
	[Export] private RayCast3D raycast;

	private int selectedIndex = 0;
	private GodotObject lastCollider = null;
	private Vector3 lastCellPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		// camera3D = GetNode<Camera3D>("Camera3D");
		// raycast = GetNode<RayCast3D>("Camera3D/RayCast3D");

		SelectBlock(5, 0);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * Sensitivity);
			camera3D.RotateX(-mouseMotion.Relative.Y * Sensitivity);
			camera3D.RotationDegrees = new Vector3(Mathf.Clamp(camera3D.RotationDegrees.X, -70, 80),
				camera3D.RotationDegrees.Y, camera3D.RotationDegrees.Z);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// gravity
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// movement
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = 0;
			velocity.Z = 0;
		}

		HandleSelectBlock();
		HandleBlockPlacement();
		HandleBlockOutline();

		Velocity = velocity;
		MoveAndSlide();
	}

	private void HandleSelectBlock()
	{
		if (Input.IsActionJustPressed("one"))
			SelectBlock(5, 0);

		if (Input.IsActionJustPressed("two"))
			SelectBlock(4, 1);

		if (Input.IsActionJustPressed("three"))
			SelectBlock(0, 2);

		if (Input.IsActionJustPressed("four"))
			SelectBlock(6, 3);
	}

	private void SelectBlock(int libraryIndex, int hotbarIndex)
	{
		selectedIndex = libraryIndex;
		EmitSignal(SignalName.BlockSelected, hotbarIndex);
	}

	private void HandleBlockPlacement()
	{
		if (Input.IsActionJustPressed("left_click"))
		{
			if (raycast.IsColliding())
			{
				if (raycast.GetCollider().HasMethod("DestroyBlock"))
				{
					raycast.GetCollider().Call("DestroyBlock", raycast.GetCollisionPoint() - raycast.GetCollisionNormal());
				}
			}
		}

		if (Input.IsActionJustPressed("right_click"))
		{
			if (raycast.IsColliding())
			{
				if (raycast.GetCollider().HasMethod("PlaceBlock"))
				{
					raycast.GetCollider().Call("PlaceBlock", raycast.GetCollisionPoint() + raycast.GetCollisionNormal(), selectedIndex);
				}
			}
		}
	}
	private void HandleBlockOutline()
	{
		if (raycast.IsColliding())
		{
			Vector3 cellPos = raycast.GetCollisionPoint() - raycast.GetCollisionNormal();
			lastCollider = raycast.GetCollider();

			if (cellPos != lastCellPos)
			{
				if (lastCollider.HasMethod("ClearOutline"))
					lastCollider.Call("ClearOutline");

				if (lastCollider.HasMethod("DrawOutline"))
					lastCollider.Call("DrawOutline", cellPos);

				lastCellPos = cellPos;
			}
		}
		else
		{
			if (lastCollider != null && lastCollider.HasMethod("ClearOutline"))
			{
				lastCollider.Call("ClearOutline");
				lastCellPos = Vector3.Zero;
			}

			lastCollider = null;
		}
	}
}
