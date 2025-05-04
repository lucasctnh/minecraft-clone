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

		// wait playing hit animation
		if (IsPlaying() && (GetCurrentAnimation() == "Run_Attack" || GetCurrentAnimation() == "Idle_Attack")) return;

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

	private void OnPlayerJump()
	{
		Play("Jump");
	}

	private void OnPlayerLand()
	{
		Play("Jump_Land");
	}
}
