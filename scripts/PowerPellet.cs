
// PowerPellet.cs
using Godot;
using System;

public partial class PowerPellet : Area2D
{
    private bool _collected = false;
    private AnimatedSprite2D _sprite;
    
    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("Sprite");
        _sprite.Play("default");
    }
    
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
