using Godot;
using System;

public partial class Hotbar : ItemList
{
	[Export] private Player player;

    public override void _EnterTree()
    {
		player.BlockSelected += SelectBlock;
    }

	public override void _ExitTree()
	{
		player.BlockSelected -= SelectBlock;
	}

	private void SelectBlock(int hotbarIndex)
	{
		Select(hotbarIndex);
	}
}
