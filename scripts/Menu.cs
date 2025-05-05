using Godot;
using System;

public partial class Menu : VBoxContainer
{
	[Export] private PackedScene gameScene;
	[Export] private Button newGameButton;
	[Export] private Button quitButton;

	public override void _Ready()
	{
		newGameButton.Pressed += OnNewGameButtonPressed;
		quitButton.Pressed += OnQuitButtonPressed;
	}

	private void OnNewGameButtonPressed()
	{
		GetTree().ChangeSceneToPacked(gameScene);
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
