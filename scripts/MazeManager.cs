using Godot;
using System;

public partial class MazeManager : Node
{
    [Export] private TileMap Walls;
    [Export] private PackedScene PelletScene;
    [Export] private PackedScene PowerPelletScene;
    
    public override void _Ready()
    {
        // In a complete implementation, this would procedurally generate pellets
        // based on the maze layout, but for the first level blueprint we'll
        // assume pellets are placed manually in the editor
    }
    
    // This is a utility function to check if a world position collides with a wall
    public bool IsWall(Vector2 worldPosition)
    {
        // Convert world position to map coordinates
        var mapPosition = Walls.LocalToMap(worldPosition);
        
        // Check if there's a tile at that position
        return Walls.GetCellSourceId(0, mapPosition) != -1;
    }
}