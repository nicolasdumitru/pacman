using Godot;
using System;

public enum GhostType
{
	Blinky,
	Pinky,
	Inky,
	Clyde
}

public enum GhostMode
{
	Chase,
	Scatter,
	Frightened,
	Eyes
}

public partial class Ghost : CharacterBody2D
{
	[Export] private GhostType Type;
	[Export] private float NormalSpeed = 80.0f;
	[Export] private float FrightenedSpeed = 40.0f;
	[Export] private float EyesSpeed = 160.0f;
	[Export] private Vector2 ScatterTarget;
	
	private AnimatedSprite2D _sprite;
	private Timer _frightenedTimer;
	private Vector2 _direction = Vector2.Left;
	private GhostMode _mode = GhostMode.Scatter;
	private Vector2 _spawnPosition;
	private int _pointValue = 200;
	
	private Pacman _pacman;
	private GhostManager _ghostManager;
	
	public bool IsFrightened => _mode == GhostMode.Frightened;
	public bool IsEyes => _mode == GhostMode.Eyes;
	
	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("Sprite");
		_frightenedTimer = GetNode<Timer>("FrightenedTimer");
		_spawnPosition = Position;
		
		_pacman = GetNode<Pacman>("/root/Main/Pacman");
		_ghostManager = GetNode<GhostManager>("/root/Main/GameManager/GhostManager");
		
		UpdateAnimation();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		// Calculate target based on mode
		Vector2 target = CalculateTarget();
		
		// Get available directions at current intersection
		var availableDirections = GetAvailableDirections();
		
		// Remove the opposite direction (no U-turns allowed)
		var oppositeDirection = -_direction;
		availableDirections.Remove(oppositeDirection);
		
		// Choose best direction based on mode
		if (_mode == GhostMode.Frightened && availableDirections.Count > 0)
		{
			// Random direction in frightened mode
			var random = new Random();
			var index = random.Next(availableDirections.Count);
			_direction = availableDirections[index];
		}
		else if (availableDirections.Count > 0)
		{
			// Choose direction closest to target
			float shortestDistance = float.MaxValue;
			
			foreach (var direction in availableDirections)
			{
				var position = Position + direction * 16; // Look ahead
				var distance = position.DistanceSquaredTo(target);
				
				if (distance < shortestDistance)
				{
					shortestDistance = distance;
					_direction = direction;
				}
			}
		}
		
		// Set velocity based on direction and mode
		float speed = _mode == GhostMode.Frightened ? FrightenedSpeed : 
					  _mode == GhostMode.Eyes ? EyesSpeed : NormalSpeed;
					  
		Velocity = _direction * speed;
		
		// Move
		var collision = MoveAndCollide(Velocity * (float)delta);
		if (collision != null)
		{
			// Handle collision with wall
			HandleWallCollision(collision);
		}
		
		UpdateAnimation();
	}
	
	private Vector2 CalculateTarget()
	{
		switch (_mode)
		{
			case GhostMode.Scatter:
				return ScatterTarget;
				
			case GhostMode.Eyes:
				return _spawnPosition;
				
			case GhostMode.Chase:
				return CalculateChaseTarget();
				
			default:
				return Vector2.Zero;
		}
	}
	
	private Vector2 CalculateChaseTarget()
	{
		var pacmanPosition = _pacman.Position;
		var pacmanDirection = _pacman.Velocity.Normalized();
		
		switch (Type)
		{
			case GhostType.Blinky: // Direct chase
				return pacmanPosition;
				
			case GhostType.Pinky: // Ambush (4 tiles ahead)
				return pacmanPosition + pacmanDirection * 4 * 16;
				
			case GhostType.Inky: // Complex targeting using Blinky's position
				var blinkyPosition = _ghostManager.GetBlinkyPosition();
				var targetPosition = pacmanPosition + pacmanDirection * 2 * 16;
				var vector = targetPosition - blinkyPosition;
				return blinkyPosition + vector * 2;
				
			case GhostType.Clyde: // Chase when far, scatter when close
				var distanceSquared = Position.DistanceSquaredTo(pacmanPosition);
				if (distanceSquared > 8 * 8 * 16 * 16) // More than 8 tiles away
					return pacmanPosition;
				else
					return ScatterTarget;
				
			default:
				return pacmanPosition;
		}
	}
	
	private System.Collections.Generic.List<Vector2> GetAvailableDirections()
	{
		var directions = new System.Collections.Generic.List<Vector2>();
		
		// Check each direction for collision
		foreach (var dir in new Vector2[] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left })
		{
			// Store current position
			var currentPosition = Position;
			
			// Test move in direction
			Position += dir;
			var collision = MoveAndCollide(Vector2.Zero, true);
			
			// If no collision, direction is available
			if (collision == null)
				directions.Add(dir);
			
			// Restore position
			Position = currentPosition;
		}
		
		return directions;
	}
	
	private void HandleWallCollision(KinematicCollision2D collision)
	{
		// Get available directions
		var availableDirections = GetAvailableDirections();
		
		// Remove the direction we just tried
		availableDirections.Remove(_direction);
		
		// Choose a new direction
		if (availableDirections.Count > 0)
		{
			var random = new Random();
			var index = random.Next(availableDirections.Count);
			_direction = availableDirections[index];
		}
	}
	
	private void UpdateAnimation()
	{
		string animationName = "";
		
		if (_mode == GhostMode.Eyes)
		{
			animationName = "eyes_";
		}
		else if (_mode == GhostMode.Frightened)
		{
			animationName = "frightened";
			
			// Check if timer is almost up
			if (_frightenedTimer.TimeLeft < 2.0)
				animationName = "frightened_ending";
		}
		else
		{
			animationName = Type.ToString().ToLower() + "_";
		}
		
		if (_mode != GhostMode.Frightened)
		{
			if (_direction == Vector2.Right)
				animationName += "right";
			else if (_direction == Vector2.Left)
				animationName += "left";
			else if (_direction == Vector2.Up)
				animationName += "up";
			else if (_direction == Vector2.Down)
				animationName += "down";
		}
		
		_sprite.Play(animationName);
	}
	
	public void SetMode(GhostMode mode, float duration = 0)
	{
		// If transitioning to frightened, reverse direction
		if (mode == GhostMode.Frightened && _mode != GhostMode.Frightened)
			_direction = -_direction;
			
		_mode = mode;
		
		if (mode == GhostMode.Frightened && duration > 0)
			_frightenedTimer.Start(duration);
			
		UpdateAnimation();
	}
	
	public void GetEaten()
	{
		SetMode(GhostMode.Eyes);
		
		// Signal to GhostManager
		_ghostManager.GhostEaten(this);
	}
	
	public void Reset()
	{
		Position = _spawnPosition;
		_direction = Vector2.Left;
		SetMode(GhostMode.Scatter);
		_pointValue = 200; // Reset point value
	}
	
	public int GetPoints()
	{
		return _pointValue;
	}
	
	public void SetPointValue(int value)
	{
		_pointValue = value;
	}
	
	private void OnFrightenedTimerTimeout()
	{
		if (_mode == GhostMode.Frightened)
			SetMode(GhostMode.Chase);
	}
	
	private void OnAreaEntered(Area2D area)
	{
		// Handle tunnel teleportation
		if (area.Name == "LeftTunnel")
			Position = new Vector2(GetViewportRect().Size.X - 16, Position.Y);
		else if (area.Name == "RightTunnel")
			Position = new Vector2(16, Position.Y);
	}
}
