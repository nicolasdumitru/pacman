// Pellet.cs
using Godot;
using System;

public partial class Pellet : Area2D
{
    private bool _collected = false;
    
    public void Collect()
    {
        if (_collected)
            return;
            
        _collected = true;
        Visible = false;
        GetNode<CollisionShape2D>("CollisionShape").SetDeferred("disabled", true);
    }
    
    public void Reset()
    {
        _collected = false;
        Visible = true;
        GetNode<CollisionShape2D>("CollisionShape").SetDeferred("disabled", false);
    }
}