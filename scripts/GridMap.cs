using Godot;
using System;

public partial class GridMap : Godot.GridMap
{
	[Export] private PackedScene blockOutline;
	private Node3D blockOutlineInstance;

	public void DestroyBlock(Vector3 worldCoordinate)
	{
		var mapCoordinate = LocalToMap(worldCoordinate);
		SetCellItem(mapCoordinate, -1);
	}

	public void PlaceBlock(Vector3 worldCoordinate, int blockIndex)
	{
		var mapCoordinate = LocalToMap(worldCoordinate);
		SetCellItem(mapCoordinate, blockIndex);
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
}
