using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public class Entity
{
    protected Vector2 _position;
    protected Scene _scene;
    protected Rectangle _rect;
    protected Point _rect_offset;
    protected string _label;

    public bool isDead = false;
    public Vector2 Position => _position;
    public Scene Scene => _scene;
    public Rectangle Rect => _rect;
    public string Label => _label;

    public Entity(Vector2 position)
    {
        _position = position;
        _rect_offset = new Point(0, 8);
        _rect = new Rectangle((int)_position.X + _rect_offset.X, (int)_position.Y + _rect_offset.Y, Grid.TileSize - _rect_offset.X * 2, Grid.TileSize - _rect_offset.Y);
    }

    public void AddToScene(Scene scene)
    {
        _scene = scene;
    }
    public void RemoveFromScene(Scene scene)
    {
        _scene = null;
    }
    public void Die()
    {
        isDead = true;
    }
    public virtual void Update()
    {

    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {

    }

    // public virtual Rectangle GetBounds()
    // {
    //     return new Rectangle((int)_position.X, (int)_position.Y, 0, 0);
    // }

    public void Translate(Vector2 translation)
    {
        _position.X += translation.X;
        _position.Y += translation.Y;
        _rect = new Rectangle((int)_position.X + _rect_offset.X, (int)_position.Y + _rect_offset.Y, Grid.TileSize - _rect_offset.X * 2, Grid.TileSize - _rect_offset.Y);
    }
}

