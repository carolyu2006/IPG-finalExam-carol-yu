using Microsoft.Xna.Framework;
using System;

namespace Template;

/// <summary>
/// Enemy that actively chases the player
/// </summary>
public class EnemyChaser : Enemy
{
    private float _speed = 80f; // pixels per second
    private float _detectionRange = 300f; // detection radius in pixels
    private Vector2 _velocity = Vector2.Zero;

    public EnemyChaser(Vector2 position, Art art, string name) : base(position, art, name)
    {
        _maxHealth = 2;
        _health = 2;
        _attack = 1;
    }

    public override void Update()
    {
        // If performing entry movement, handle it first and skip AI until complete
        if (HandleEntry())
        {
            ClampToScreenBounds();
            return;
        }

        // Find player
        Player player = FindPlayer();
        if (player == null)
        {
            base.Update();
            return;
        }

        // Calculate distance to player
        Vector2 playerCenter = player.Position + new Vector2(Grid.TileSize / 2, Grid.TileSize / 2);
        Vector2 thisCenter = _position + new Vector2(Grid.TileSize / 2, Grid.TileSize / 2);
        Vector2 toPlayer = playerCenter - thisCenter;
        float distance = toPlayer.Length();

        // If within detection range, chase the player
        if (distance > 0 && distance <= _detectionRange)
        {
            // Normalize direction and apply speed
            Vector2 direction = toPlayer / distance;
            _velocity = direction * _speed * ServiceLocator.DeltaSeconds;

            // Apply movement with collision checking per axis
            if (_scene != null)
            {
                if (Math.Abs(_velocity.X) > 0.1f)
                {
                    Vector2 vx = new Vector2(_velocity.X, 0);
                    vx = _scene.CheckForGridCollision(this, vx);
                    if (vx.X != 0) Translate(new Vector2(vx.X, 0));
                }
                if (Math.Abs(_velocity.Y) > 0.1f)
                {
                    Vector2 vy = new Vector2(0, _velocity.Y);
                    vy = _scene.CheckForGridCollision(this, vy);
                    if (vy.Y != 0) Translate(new Vector2(0, vy.Y));
                }
            }
            else
            {
                Translate(_velocity);
            }

            ClampToScreenBounds();
        }

        //take damage on contact with player
        // var hit = _scene.WhichEntityColliding(this, "player");
        // if (hit is Player)
        // {
        //     player.TakeDamage(1);
        //     Die();
        //     return;
        // }

        base.Update();
    }

    private Player FindPlayer()
    {
        if (_scene == null) return null;
        return _scene.GetPlayer();
    }
}
