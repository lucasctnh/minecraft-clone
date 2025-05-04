using Godot;
using System;

public partial class Menu : VBoxContainer
{
	[Export] private PackedScene gameScene;
	[Export] private Button newGameButton;
	[Export] private Button loadButton;
	[Export] private Button quitButton;

	public override void _Ready()
	{
		newGameButton.Pressed += OnNewGameButtonPressed;
		loadButton.Pressed += OnLoadButtonPressed;
		quitButton.Pressed += OnQuitButtonPressed;
	}

	private void OnNewGameButtonPressed()
	{
		GetTree().ChangeSceneToPacked(gameScene);
	}

	private void OnLoadButtonPressed()
	{
		throw new NotImplementedException();
	}

	private void OnQuitButtonPressed()
	{
		throw new NotImplementedException();
	}
}
