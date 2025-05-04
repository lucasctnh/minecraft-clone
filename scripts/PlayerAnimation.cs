using Godot;
using System;

public partial class PlayerAnimation : AnimationPlayer
{
	[Export] private Player player;

	private bool isMoving = false;

	public override void _EnterTree()
	{
		player.OnMove += OnPlayerMove;
		player.OnHit += OnPlayerHit;
		player.OnJump += OnPlayerJump;
		player.OnLand += OnPlayerLand;
	}

	public override void _ExitTree()
	{
		player.OnMove -= OnPlayerMove;
		player.OnHit -= OnPlayerHit;
		player.OnJump -= OnPlayerJump;
		player.OnLand -= OnPlayerLand;
	}

	private void OnPlayerMove(Vector3 velocity)
	{
		isMoving = velocity != Vector3.Zero;

		// wait playing other animations
		string currentAnim = IsPlaying() ? GetCurrentAnimation() : "";
		if (IsPlaying() && currentAnim != "Run" && currentAnim != "Idle")
			return;

		if (isMoving)
			Play("Run");
		else
			Play("Idle");
	}

	private void OnPlayerHit(bool isHitting)
	{
		if (isHitting)
		{
			if (isMoving)
				Play("Run_Attack");
			else
				Play("Idle_Attack");
		}
	}

	private async void OnPlayerJump()
	{
		Play("Jump");

		await ToSignal(this, SignalName.AnimationFinished);

		GetAnimation("Jump_Idle").LoopMode = Animation.LoopModeEnum.Linear;
		Play("Jump_Idle");
	}

	private void OnPlayerLand()
	{
		Play("Jump_Land");
	}
}
