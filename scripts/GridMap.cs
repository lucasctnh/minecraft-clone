using Godot;
using System;
using System.Collections.Generic;

public partial class GridMap : Godot.GridMap
{
	[Signal]
	public delegate void BlockBreakingChangedEventHandler();

	[Export] private PackedScene blockOutline;
	[Export] private PackedScene blockBreaking;

	private Node3D blockOutlineInstance;
	private Dictionary<Vector3I, BlockBreaking> blockBreakingInstances;

	public override void _Ready()
	{
		blockOutlineInstance = null;
		blockBreakingInstances = new Dictionary<Vector3I, BlockBreaking>();
	}

	public void DestroyBlock(Vector3 worldCoordinate)
	{
		var mapCoordinate = LocalToMap(worldCoordinate);
		SetCellItem(mapCoordinate, -1);
	}

	public bool PlaceBlock(Vector3 worldCoordinate, int blockIndex)
	{
		var mapCoordinate = LocalToMap(worldCoordinate);
		Vector3 blockPosition = MapToLocal(mapCoordinate);

		// collision check to avoid placing blocks inside player
		var space = GetWorld3D().DirectSpaceState;

		var parameters = new PhysicsShapeQueryParameters3D();
		var shape = new BoxShape3D();
		shape.Size = new Vector3(1.9f, 1.9f, 1.9f);
		parameters.Shape = shape;
		parameters.Transform = new Transform3D(Basis.Identity, blockPosition);

		// check collision with all layers containing player
		parameters.CollideWithAreas = false;
		parameters.CollideWithBodies = true;
		parameters.CollisionMask = 0b10;  // layer 2

		var result = space.IntersectShape(parameters);

		// check for any player collisions
		foreach (var collision in result)
		{
			var collider = collision["collider"].As<Node>();
			if (collider is CharacterBody3D)
				return false;
		}

		SetCellItem(mapCoordinate, blockIndex);
		return true;
	}

	public void DrawOutline(Vector3 worldCoordinate)
	{
		if (blockOutlineInstance != null) return;

		blockOutlineInstance = blockOutline.Instantiate() as Node3D;
		AddChild(blockOutlineInstance);

		blockOutlineInstance.GlobalPosition = MapToLocal(LocalToMap(worldCoordinate));
	}

	public void ClearOutline()
	{
		if (blockOutlineInstance == null) return;

		blockOutlineInstance.QueueFree();
		blockOutlineInstance = null;
	}

	public void StartBreaking(Vector3 worldCoordinate)
	{
		Vector3I mapCoordinate = LocalToMap(worldCoordinate);
		int cellItemIndex = GetCellItem(mapCoordinate);

		if (cellItemIndex == -1) return;
		if (blockBreakingInstances.ContainsKey(mapCoordinate)) return;

		var instance = blockBreaking.Instantiate() as BlockBreaking;
		AddChild(instance);

		instance.GlobalPosition = MapToLocal(mapCoordinate);

		blockBreakingInstances.Add(mapCoordinate, instance);
	}

	public void ChangeBreakingProgress(int frame, Vector3 worldCoordinate)
	{
		Vector3I mapCoordinate = LocalToMap(worldCoordinate);

		if (!blockBreakingInstances.ContainsKey(mapCoordinate))
		{
			StartBreaking(worldCoordinate);
			EmitSignal(SignalName.BlockBreakingChanged);

			if (!blockBreakingInstances.ContainsKey(mapCoordinate))
				return;
		}

		BlockBreaking blockBreakingInstance = blockBreakingInstances[mapCoordinate];

		if (blockBreakingInstance != null)
			blockBreakingInstance.ChangeFrame(frame);
	}

	public void StopBreaking()
	{
		foreach (BlockBreaking instance in blockBreakingInstances.Values)
		{
			if (instance != null)
				instance.QueueFree();
		}

		blockBreakingInstances.Clear();
	}
}
