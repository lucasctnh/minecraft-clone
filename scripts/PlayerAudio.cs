using Godot;
using System;

public partial class PlayerAudio : AudioStreamPlayer
{
	[Export] private Player player;

	[Export] private AudioStream walkAudio;
	[Export] private AudioStream selectAudio;
	[Export] private AudioStream hitAudio;
	[Export] private AudioStream placeAudio;
	[Export] private AudioStream breakAudio;
	[Export] private AudioStream landAudio;

	public override void _EnterTree()
	{
		player.OnMove += OnWalk;
		player.BlockSelected += OnBlockSelected;
		player.OnHit += OnHit;
		player.OnPlace += OnPlace;
		player.BlockBreak += OnBreak;
		player.OnLand += OnLand;
	}

	public override void _ExitTree()
	{
		player.OnMove -= OnWalk;
		player.BlockSelected -= OnBlockSelected;
		player.OnHit -= OnHit;
		player.OnPlace -= OnPlace;
		player.BlockBreak -= OnBreak;
		player.OnLand -= OnLand;
	}

	private void OnWalk(Vector3 velocity)
	{
		bool isMoving = velocity != Vector3.Zero;
		if (isMoving)
		{
			if (walkAudio != null && !IsPlaying())
			{
				Stream = walkAudio;
				Play();
			}
		}
		else
		{
			if (IsPlaying() && Stream == walkAudio)
				Stop();
		}
	}

	private void OnBlockSelected(int hotbarIndex)
	{
		if (selectAudio != null)
		{
			Stream = selectAudio;
			Play();
		}
	}

	private void OnHit(bool isHitting)
	{
		if (isHitting)
		{
			if (hitAudio != null && (!IsPlaying() || (IsPlaying() && Stream == walkAudio)))
			{
				Stream = hitAudio;
				Play();
			}
		}
		else
		{
			if (IsPlaying() && Stream == hitAudio)
				Stop();
		}
	}

	private void OnPlace(bool isPlacing)
	{
		if (isPlacing)
		{
			if (placeAudio != null)
			{
				Stream = placeAudio;
				Play();
			}
		}
		else
		{
			if (IsPlaying() && Stream == placeAudio)
				Stop();
		}
	}

	private void OnBreak()
	{
		if (breakAudio != null)
		{
			Stream = breakAudio;
			Play();
		}
	}

	private void OnLand()
	{
		if (landAudio != null && (!IsPlaying() || (IsPlaying() && Stream == walkAudio)))
		{
			Stream = landAudio;
			Play();
		}
	}
}
