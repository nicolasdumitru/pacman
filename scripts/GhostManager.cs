using Godot;
using System;
using System.Collections.Generic;

public partial class GhostManager : Node
{
	[Export] private float FrightenedDuration = 7.0f;
	
	private List<Ghost> _ghosts = new List<Ghost>();
	private Ghost _blinky;
	private Timer _modeTimer;
	private int _modeIndex = 0;
	private int _ghostEatenCount = 0;
	
	// Mode timing for first level (in seconds)
	private float[] _modeDurations = new float[] { 7, 20, 7, 20, 5, 20, 5, float.MaxValue };
	
	public override void _Ready()
	{
		_modeTimer = GetNode<Timer>("ModeTimer");
		
		// Get ghost references
		_blinky = GetNode<Ghost>("/root/Main/Ghosts/Blinky");
		var pinky = GetNode<Ghost>("/root/Main/Ghosts/Pinky");
		var inky = GetNode<Ghost>("/root/Main/Ghosts/Inky");
		var clyde = GetNode<Ghost>("/root/Main/Ghosts/Clyde");
		
		_ghosts.Add(_blinky);
		_ghosts.Add(pinky);
		_ghosts.Add(inky);
		_ghosts.Add(clyde);
		
		// Start ghost mode cycle
		StartModeCycle();
	}
	
	private void StartModeCycle()
	{
		_modeIndex = 0;
		ChangeMode();
	}
	
	private void ChangeMode()
	{
		var duration = _modeDurations[_modeIndex];
		var isScatterMode = _modeIndex % 2 == 0; // Even indices are scatter modes
		
		foreach (var ghost in _ghosts)
		{
			if (ghost.IsFrightened || ghost.IsEyes)
				continue; // Don't change mode if ghost is frightened or returning to spawn
				
			ghost.SetMode(isScatterMode ? GhostMode.Scatter : GhostMode.Chase);
		}
		
		_modeTimer.Start(duration);
		_modeIndex = (_modeIndex + 1) % _modeDurations.Length;
	}
	
	public void PowerPelletCollected()
	{
		_ghostEatenCount = 0;
		
		foreach (var ghost in _ghosts)
		{
			if (!ghost.IsEyes) // Don't affect ghosts returning to spawn
				ghost.SetMode(GhostMode.Frightened, FrightenedDuration);
		}
	}
	
	public void GhostEaten(Ghost ghost)
	{
		_ghostEatenCount++;
		
		// Double points for each ghost eaten during a single power pellet duration
		var pointValue = 200 * (int)Math.Pow(2, _ghostEatenCount - 1);
		ghost.SetPointValue(pointValue);
	}
	
	public Vector2 GetBlinkyPosition()
	{
		return _blinky.Position;
	}
	
	public void Reset()
	{
		_modeTimer.Stop();
		
		foreach (var ghost in _ghosts)
		{
			ghost.Reset();
		}
		
		StartModeCycle();
	}
	
	private void OnModeTimerTimeout()
	{
		ChangeMode();
	}
}
