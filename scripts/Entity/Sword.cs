using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public class Sword : SpriteEntity
{
    private float _life = 0.18f; // seconds
    private float _timer = 0f;
    private Player _owner;
    private Directions _dir;
    private float _speed = 400f; // pixels per second
    private Vector2 _velocity = Vector2.Zero;

    public Sword(Vector2 position, Art art, Player owner, Directions dir) : base(position, art)
    {
        _label = "sword";
        _texture = AssetManager.GetTexture(art);
        _owner = owner;

        // Set collision rect to a smaller hit area depending on direction
        _dir = dir;
        // compute velocity based on direction
        switch (_dir)
        {
            case Directions.Left: _velocity = new Vector2(-_speed, 0); break;
            case Directions.Right: _velocity = new Vector2(_speed, 0); break;
            case Directions.Up: _velocity = new Vector2(0, -_speed); break;
            case Directions.Down: _velocity = new Vector2(0, _speed); break;
        }
        switch (dir)
        {
            case Directions.Left:
                _rect_offset = new Point(-Grid.TileSize / 2, Grid.TileSize / 4);
                break;
            case Directions.Right:
                _rect_offset = new Point(Grid.TileSize / 2, Grid.TileSize / 4);
                break;
            case Directions.Up:
                _rect_offset = new Point(Grid.TileSize / 4, -Grid.TileSize / 2);
                break;
            case Directions.Down:
            default:
                _rect_offset = new Point(Grid.TileSize / 4, Grid.TileSize / 2);
                break;
        }
        _rect = new Rectangle((int)_position.X + _rect_offset.X, (int)_position.Y + _rect_offset.Y, Grid.TileSize, Grid.TileSize);
    }

    public Sword(Vector2 position, Texture2D texture, Player owner, Directions dir) : base(position, Art.sword)
    {
        _label = "sword";
        _texture = texture;
        _owner = owner;

        _dir = dir;
        switch (_dir)
        {
            case Directions.Left: _velocity = new Vector2(-_speed, 0); break;
            case Directions.Right: _velocity = new Vector2(_speed, 0); break;
            case Directions.Up: _velocity = new Vector2(0, -_speed); break;
            case Directions.Down: _velocity = new Vector2(0, _speed); break;
        }
        switch (dir)
        {
            case Directions.Left:
                _rect_offset = new Point(-Grid.TileSize / 2, Grid.TileSize / 4);
                break;
            case Directions.Right:
                _rect_offset = new Point(Grid.TileSize / 2, Grid.TileSize / 4);
                break;
            case Directions.Up:
                _rect_offset = new Point(Grid.TileSize / 4, -Grid.TileSize / 2);
                break;
            case Directions.Down:
            default:
                _rect_offset = new Point(Grid.TileSize / 4, Grid.TileSize / 2);
                break;
        }
        _rect = new Rectangle((int)_position.X + _rect_offset.X, (int)_position.Y + _rect_offset.Y, Grid.TileSize, Grid.TileSize);
    }

    public override void Update()
    {
        // move sword forward
        if (_velocity != Vector2.Zero)
        {
            Translate(_velocity * ServiceLocator.DeltaSeconds);
        }

        // If the sword leaves the visible scene area, remove it
        if (_scene != null)
        {
            float left = Scene.SceneOffset.X;
            float top = Scene.SceneOffset.Y;
            float right = left + Scene.GameSceneSize.X;
            float bottom = top + Scene.GameSceneSize.Y;

            if (_position.X < left + Grid.TileSize || _position.X > right - Grid.TileSize || _position.Y < top + Grid.TileSize || _position.Y > bottom - Grid.TileSize)
            {
                AnimationEntity dieAnim = new AnimationEntity(_position, AssetManager.DeathFrames, 0.12f, false)
                {
                    OnFinished = () =>
                    {
                        Die();
                        return;
                    }
                };
                _scene?.AddEntity(dieAnim);
                Die();
                return;
            }
        }

        // Check collision with enemies and deal damage
        if (_scene != null)
        {
            var hit = _scene.WhichEntityColliding(this, "enemy");
            if (hit is Enemy e && _owner != null)
            {
                e.TakeDamage(_owner.Attack);
                // optional: make the sword disappear after hit
                Die();
                return;
            }
        }

        base.Update();
    }


}
