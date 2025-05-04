using Godot;
using System;

public partial class GridMap : Godot.GridMap
{
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
}
