using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Signal] public delegate void BlockSelectedEventHandler(int hotbarIndex);
	[Signal] public delegate void BlockBreakingEventHandler(int progress);

	[Signal] public delegate void OnMoveEventHandler(Vector3 velocity);
	[Signal] public delegate void OnHitEventHandler(bool isHitting);
	[Signal] public delegate void OnJumpEventHandler();
	[Signal] public delegate void OnLandEventHandler();

	[Export] public float Speed = 8f;
	[Export] public float JumpVelocity = 10f;
	[Export] public float Sensitivity = 0.002f;
	[Export] public Vector2 LookRange = new Vector2(-90, 90);
	[Export] public float BlockDestroyDelay = 0.1f;
	[Export] public float BlockPlacementDelay = 0.1f;

	[Export] private Camera3D fpsCamera;
	[Export] private RayCast3D fpsRaycast;
	[Export] private Camera3D tpsCamera;
	[Export] private RayCast3D tpsRaycast;
	[Export] private GridMap gridMap;

	private Camera3D Camera => isFps ? fpsCamera : tpsCamera;
	private RayCast3D Raycast => isFps ? fpsRaycast : tpsRaycast;

	private int selectedIndex = 0;
	private GodotObject lastOutlineCollider = null;
	private Vector3 lastOutlineCellPos = Vector3.Zero;
	private float blockDestroyTimer = 0f;
	private float blockPlacementTimer = 0f;
	private int blockDestroyProgress = 0;
	private bool hasJustJumped = false;
	private bool isFps = true;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		// camera3D = GetNode<Camera3D>("Camera3D");
		// raycast = GetNode<RayCast3D>("Camera3D/RayCast3D");

		SelectBlock(5, 0);
		blockPlacementTimer = BlockPlacementDelay;
		blockDestroyTimer = 0;
		blockDestroyProgress = 0;

		fpsCamera.Current = true;
		tpsCamera.Current = false;
	}

	public override void _EnterTree()
	{
		gridMap.BlockBreakingChanged += ResetBlockBreaking;
	}

	public override void _ExitTree()
	{
		gridMap.BlockBreakingChanged -= ResetBlockBreaking;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * Sensitivity);

			Camera.RotateX(-mouseMotion.Relative.Y * Sensitivity);
			Camera.RotationDegrees = new Vector3(Mathf.Clamp(Camera.RotationDegrees.X, LookRange.X, LookRange.Y),
				Camera.RotationDegrees.Y, Camera.RotationDegrees.Z);
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

			hasJustJumped = true;
			EmitSignal(SignalName.OnJump);
		}
		if (IsOnFloor() && velocity.Y == 0 && hasJustJumped)
		{
			hasJustJumped = false;
			EmitSignal(SignalName.OnLand);
		}

		// change camera
		if (Input.IsActionJustPressed("toggle_camera"))
		{
			isFps = !isFps;

			if (isFps)
			{
				fpsCamera.Current = true;
				tpsCamera.Current = false;
			}
			else
			{
				fpsCamera.Current = false;
				tpsCamera.Current = true;
			}
		}

		// adjust tps camera position
		if (isFps == false)
		{
			float normalizedLook = Mathf.InverseLerp(LookRange.X, 0, tpsCamera.RotationDegrees.X);
			normalizedLook = Mathf.Lerp(-1.0f, 1.0f, normalizedLook);

			float newPosZ = tpsCamera.Position.Z + normalizedLook;
			tpsCamera.Position = new Vector3(tpsCamera.Position.X, tpsCamera.Position.Y, Mathf.Clamp(newPosZ, -1f, 5.8f));
		}

		// calculate movement
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

		// block handling
		HandleSelectBlock();

		if (Raycast.IsColliding())
		{
			DrawBlockOutline();
			HandleBlockPlacement();
		}
		else
			ClearOutline();

		// apply movement
		Velocity = velocity;
		MoveAndSlide();

		EmitSignal(SignalName.OnMove, velocity);
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
		if (Input.IsActionPressed("left_click"))
		{
			EmitSignal(SignalName.OnHit, true);

			Vector3 cellPos = Raycast.GetCollisionPoint() - Raycast.GetCollisionNormal();
			GodotObject collider = Raycast.GetCollider();

			blockDestroyTimer += (float)GetPhysicsProcessDeltaTime();

			// calculate breaking as a value from 0 to 6 based on the timer
			// where breaking only starts at 1 and ends at 6
			if (collider.HasMethod("StartBreaking"))
			{
				int newProgress = Mathf.Min(6, Mathf.FloorToInt(blockDestroyTimer / BlockDestroyDelay * 7));
				if (newProgress != blockDestroyProgress)
				{
					if (blockDestroyProgress == 0 && newProgress == 1)
						collider.Call("StartBreaking", cellPos);
					else if (newProgress > 1)
						collider.Call("ChangeBreakingProgress", newProgress - 1, cellPos);

					blockDestroyProgress = newProgress;
				}
			}

			if (blockDestroyTimer > BlockDestroyDelay && collider.HasMethod("DestroyBlock"))
			{
				gridMap.StopBreaking();
				collider.Call("DestroyBlock", cellPos);
				blockDestroyTimer = 0;
			}
		}
		else
		{
			gridMap.StopBreaking();

			// make sure the click while not holding is NOT instantaneous
			blockDestroyTimer = 0;
		}

		if (Input.IsActionPressed("right_click"))
		{
			EmitSignal(SignalName.OnHit, true);

			blockPlacementTimer += (float)GetPhysicsProcessDeltaTime();

			if (blockPlacementTimer > BlockPlacementDelay && Raycast.GetCollider().HasMethod("PlaceBlock"))
			{
				Raycast.GetCollider().Call("PlaceBlock", Raycast.GetCollisionPoint() + Raycast.GetCollisionNormal(), selectedIndex);
				blockPlacementTimer = 0;
			}
		}
		else
		{
			// make sure the click while not holding is instantaneous
			blockPlacementTimer = BlockPlacementDelay;
		}

		if (Input.IsActionPressed("left_click") == false && Input.IsActionPressed("right_click") == false)
		{
			EmitSignal(SignalName.OnHit, false);
		}
	}

	private void DrawBlockOutline()
	{
		Vector3 cellPos = Raycast.GetCollisionPoint() - Raycast.GetCollisionNormal();
		lastOutlineCollider = Raycast.GetCollider();

		if (cellPos != lastOutlineCellPos)
		{
			if (lastOutlineCollider.HasMethod("ClearOutline"))
				lastOutlineCollider.Call("ClearOutline");

			if (lastOutlineCollider.HasMethod("DrawOutline"))
				lastOutlineCollider.Call("DrawOutline", cellPos);

			lastOutlineCellPos = cellPos;
		}
	}

	private void ClearOutline()
	{
		if (lastOutlineCollider != null && lastOutlineCollider.HasMethod("ClearOutline"))
		{
			lastOutlineCollider.Call("ClearOutline");
			lastOutlineCellPos = Vector3.Zero;
		}

		lastOutlineCollider = null;
	}

	private void ResetBlockBreaking()
	{
		if (blockDestroyProgress > 0)
		{
			blockDestroyProgress = 0;
			blockDestroyTimer = 0;
		}

		gridMap.StopBreaking();
	}
}
