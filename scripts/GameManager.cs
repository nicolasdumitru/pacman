using Godot;
using System;

public partial class GameManager : Node
{
	[Export] private int Lives = 3;
	[Export] private PackedScene FruitScene;
	
	private int _score = 0;
	private int _highScore = 0;
	private int _currentLives;
	private int _totalPellets = 240;
	private int _remainingPellets;
	private Label _scoreLabel;
	private HBoxContainer _livesContainer;
	private Panel _gameOverPanel;
	private Vector2 _pacmanSpawnPosition;
	private GhostManager _ghostManager;
	private Pacman _pacman;
	private bool _gameOver = false;
	
	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("/root/Main/UI/Score");
		_livesContainer = GetNode<HBoxContainer>("/root/Main/UI/Lives");
		_gameOverPanel = GetNode<Panel>("/root/Main/UI/GameOverPanel");
		_pacman = GetNode<Pacman>("/root/Main/Pacman");
		_ghostManager = GetNode<GhostManager>("GhostManager");
		
		_pacmanSpawnPosition = _pacman.Position;
		_currentLives = Lives;
		_remainingPellets = _totalPellets;
		
		_gameOverPanel.Visible = false;
		UpdateUI();
	}
	
	public void AddScore(int points)
	{
		_score += points;
		
		if (_score > _highScore)
			_highScore = _score;
			
		UpdateUI();
	}
	
	public void PelletCollected()
	{
		_remainingPellets--;
			
		CheckLevelComplete();
	}
	
	public void PowerPelletCollected()
	{
		_ghostManager.PowerPelletCollected();
		PelletCollected(); // Also counts as a regular pellet
	}
	
	public void OnPacmanDeath()
	{
		_currentLives--;
		
		if (_currentLives <= 0)
		{
			GameOver();
		}
		else
		{
			// Reset Pac-Man and ghosts
			_pacman.Reset(_pacmanSpawnPosition);
			_ghostManager.Reset();
			UpdateUI();
		}
	}
	
	private void CheckLevelComplete()
	{
		if (_remainingPellets <= 0)
		{
			// In a full game, we would load the next level here
			// For this blueprint, we'll just reset the current level
			ResetLevel();
		}
	}
	
	private void ResetLevel()
	{
		// Respawn all pellets
		var pellets = GetNode<Node2D>("/root/Main/Pellets");
		foreach (var child in pellets.GetChildren())
		{
			if (child is Pellet pellet)
				pellet.Reset();
		}
		
		var powerPellets = GetNode<Node2D>("/root/Main/PowerPellets");
		foreach (var child in powerPellets.GetChildren())
		{
			if (child is PowerPellet powerPellet)
				powerPellet.Reset();
		}
		
		_remainingPellets = _totalPellets;
		
		// Reset Pac-Man and ghosts
		_pacman.Reset(_pacmanSpawnPosition);
		_ghostManager.Reset();
	}
	
	private void GameOver()
	{
		_gameOver = true;
		_gameOverPanel.Visible = true;
	}
	
	private void UpdateUI()
	{
		_scoreLabel.Text = $"SCORE: {_score}\nHIGH SCORE: {_highScore}";
		
		// Update lives display
		foreach (var child in _livesContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		for (int i = 0; i < _currentLives; i++)
		{
			var lifeIcon = new TextureRect();
			lifeIcon.Texture = ResourceLoader.Load<Texture2D>("res://assets/sprites/pacman_life.png");
			_livesContainer.AddChild(lifeIcon);
		}
	}
	
	public void RestartGame()
	{
		_score = 0;
		_currentLives = Lives;
		_remainingPellets = _totalPellets;
		_gameOver = false;
		_gameOverPanel.Visible = false;
		
		ResetLevel();
		UpdateUI();
	}
}
