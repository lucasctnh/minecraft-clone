using Godot;
using System;

public partial class Hotbar : ItemList
{
	[Export] private Player player;
	[Export] private AudioStreamPlayer selectAudio;

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
		if (selectAudio != null && selectAudio.Playing == false)
			selectAudio.Play();

		Select(hotbarIndex);
	}
}
