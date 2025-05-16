using Godot;
using System;

public partial class Pacman : CharacterBody2D {
	[Export] private float Speed = 100.0f;
	[Export] private PackedScene DeathEffectScene;

	private AnimatedSprite2D _sprite;
	private CollisionShape2D _collisionShape;
	private Vector2 _direction = Vector2.Right;
	private Vector2 _nextDirection = Vector2.Right;
	private bool _isDead = false;
	private Timer _deathTimer;

	private GameManager _gameManager;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("Sprite");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape");
		_deathTimer = GetNode<Timer>("DeathTimer");
		_gameManager = GetNode<GameManager>("/root/Main/GameManager");

		// Start animation
		_sprite.Play("right");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead)
			return;

		// Handle input
		if (Input.IsActionPressed("move_right"))
			_nextDirection = Vector2.Right;
		else if (Input.IsActionPressed("move_left"))
			_nextDirection = Vector2.Left;
		else if (Input.IsActionPressed("move_up"))
			_nextDirection = Vector2.Up;
		else if (Input.IsActionPressed("move_down"))
			_nextDirection = Vector2.Down;

		// Try to change direction
		if (_nextDirection != _direction)
		{
			// Store current position
			var currentPosition = Position;

			// Test move in next direction
			Position += _nextDirection;
			var collision = MoveAndCollide(Vector2.Zero, true);

			// If no collision, change direction
			if (collision == null)
			{
				_direction = _nextDirection;
				SetAnimation();
			}

			// Restore position
			Position = currentPosition;
		}

		// Move in current direction
		Velocity = _direction * Speed;
		MoveAndSlide();
	}

	private void SetAnimation()
	{
		if (_direction == Vector2.Right)
			_sprite.Play("face_right");
		else if (_direction == Vector2.Left)
			_sprite.Play("face_left");
		else if (_direction == Vector2.Up)
			_sprite.Play("face_up");
		else if (_direction == Vector2.Down)
			_sprite.Play("face_down");
	}

	public void Die()
	{
		_isDead = true;
		_sprite.Play("death");
		_collisionShape.SetDeferred("disabled", true);
		_deathTimer.Start();
	}

	private void OnDeathTimerTimeout()
	{
		_gameManager.OnPacmanDeath();
	}

	public void Reset(Vector2 position)
	{
		Position = position;
		_direction = Vector2.Right;
		_nextDirection = Vector2.Right;
		_isDead = false;
		_sprite.Play("right");
		_collisionShape.SetDeferred("disabled", false);
	}

	public void OnGhostCollision(Ghost ghost)
	{
		if (ghost.IsFrightened)
		{
			ghost.GetEaten();
			_gameManager.AddScore(ghost.GetPoints());
		}
		else if (!ghost.IsEyes)
		{
			Die();
		}
	}

	public void OnPelletCollision(Pellet pellet)
	{
		pellet.Collect();
		_gameManager.AddScore(10);
		_gameManager.PelletCollected();
	}

	public void OnPowerPelletCollision(PowerPellet powerPellet)
	{
		powerPellet.Collect();
		_gameManager.AddScore(50);
		_gameManager.PowerPelletCollected();
	}
}
