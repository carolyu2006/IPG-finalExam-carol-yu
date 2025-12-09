using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

/// <summary>
/// Projectile that can be shot by player or enemies
/// </summary>
public class Projectile : SpriteEntity
{
    private float _lifeTime = 3f; // seconds before despawning
    private float _timer = 0f;
    private Entity _owner;
    private Directions _dir;
    private float _speed = 300f; // pixels per second
    private Vector2 _velocity = Vector2.Zero;
    private int _damage = 1;

    public Projectile(Vector2 position, Directions dir, Entity owner, int damage = 1, float speed = 300f)
        : base(position, Art.Pixel)
    {
        _label = "projectile";
        _owner = owner;
        _dir = dir;
        _damage = damage;
        _speed = speed;

        // Set velocity based on direction
        switch (_dir)
        {
            case Directions.Left: _velocity = new Vector2(-_speed, 0); break;
            case Directions.Right: _velocity = new Vector2(_speed, 0); break;
            case Directions.Up: _velocity = new Vector2(0, -_speed); break;
            case Directions.Down: _velocity = new Vector2(0, _speed); break;
        }

        // Use bullet texture if available, otherwise use a colored pixel
        try
        {
            _texture = AssetManager.GetTexture(Art.Coin); // Using coin as placeholder
        }
        catch
        {
            _texture = AssetManager.GetTexture(Art.Pixel);
        }

        // Small collision rect for projectile
        _rect = new Rectangle((int)_position.X, (int)_position.Y, 16, 16);
    }

    public override void Update()
    {
        // Move projectile
        Translate(_velocity * ServiceLocator.DeltaSeconds);

        // Update collision rect
        _rect.X = (int)_position.X;
        _rect.Y = (int)_position.Y;

        // Check lifetime
        _timer += ServiceLocator.DeltaSeconds;
        if (_timer >= _lifeTime)
        {
            Die();
            return;
        }

        // Check bounds - despawn if off screen
        if (_position.X < -50 || _position.X > Game1.ScreenSize.X + 50 ||
            _position.Y < -50 || _position.Y > Game1.ScreenSize.Y + 50)
        {
            Die();
            return;
        }

        // Check collision with enemies if shot by player
        if (_owner is Player && _scene != null)
        {
            Entity hit = _scene.WhichEntityColliding(this, "enemy");
            if (hit is Enemy enemy && !enemy.isDead)
            {
                enemy.TakeDamage(_damage);
                Die();
                return;
            }
        }

        // Check collision with player if shot by enemy
        if (_owner is Enemy && _scene != null)
        {
            Player player = ServiceLocator.GameState?.Player;
            if (player != null && _rect.Intersects(player.Rect))
            {
                player.TakeDamage(_damage);
                Die();
                return;
            }
        }

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_texture != null)
        {
            // Draw projectile with slight scaling
            Rectangle destRect = new Rectangle((int)_position.X, (int)_position.Y, 24, 24);
            spriteBatch.Draw(_texture, destRect, Color.Yellow);
        }
    }
}
