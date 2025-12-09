using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Template;

// Projectile for the shooter enemy
public class Bullet : SpriteEntity
{
    private Vector2 _velocity;
    private float _lifetime = 3f; // seconds
    private float _timer = 0f;

    public Bullet(Vector2 position, Vector2 direction, float speed) : base(position, Art.Enemy)
    {
        _label = "bullet";
        _velocity = direction * speed;
        _texture = AssetManager.ShooterBullet;

        // Smaller collision box for bullets
        _rect_offset = new Point(Grid.TileSize / 4, Grid.TileSize / 4);
        _rect = new Rectangle(
            (int)_position.X + _rect_offset.X,
            (int)_position.Y + _rect_offset.Y,
            Grid.TileSize / 2,
            Grid.TileSize / 2
        );
    }

    public override void Update()
    {
        _timer += ServiceLocator.DeltaSeconds;
        if (_timer >= _lifetime)
        {
            // Play death animation at bullet position, then remove the bullet when animation finishes.
            // if (_scene != null)
            // {
            AnimationEntity dieAnim = new AnimationEntity(_position, AssetManager.DeathFrames, 0.12f, false)
            {
                OnFinished = () =>
                {
                    Die();
                    return;
                }
            };
            _scene.AddEntity(dieAnim);
            // }
            // else
            // {
            //     // No scene to show animation in — just die immediately.
            //     Die();
            // }

            return;
        }

        Vector2 nextPos = _position + _velocity * ServiceLocator.DeltaSeconds;

        // Check collision with grid
        if (_scene != null)
        {
            Vector2 velocityX = new Vector2(_velocity.X * ServiceLocator.DeltaSeconds, 0);
            velocityX = _scene.CheckForGridCollision(this, velocityX);

            Vector2 velocityY = new Vector2(0, _velocity.Y * ServiceLocator.DeltaSeconds);
            velocityY = _scene.CheckForGridCollision(this, velocityY);

            // Die if hitting a wall
            // If the scene collision reduced the allowed movement in either axis,
            // consider the bullet blocked and destroy it.
            if (Math.Abs(velocityX.X) < Math.Abs(_velocity.X * ServiceLocator.DeltaSeconds) ||
                Math.Abs(velocityY.Y) < Math.Abs(_velocity.Y * ServiceLocator.DeltaSeconds))
            {
                Die();
                return;
            }

            // Check collision with player
            var hit = _scene.WhichEntityColliding(this, "player");
            if (hit is Player player)
            {
                player.TakeDamage(1);
                Die();
                return;
            }
        }

        // Move according to velocity (pixels per second) scaled by frame delta.
        Translate(_velocity * ServiceLocator.DeltaSeconds);

        // Die if out of bounds
        if (_position.X < -100 || _position.X > Game1.ScreenSize.X + 100 ||
            _position.Y < -100 || _position.Y > Game1.ScreenSize.Y + 100)
        {
            Die();
        }

        base.Update();
    }
}

public class EnemyShooter : Enemy
{
    private Directions _currentDirection = Directions.Down;
    private int _animFrame = 0;
    private float _animDistance = 0f;
    private const float AnimStep = 35f;

    private float _shootTimer = 0f;
    private const float ShootInterval = 2f; // shoot every 2 seconds
    private const float BulletSpeed = 300f;
    private const float DetectionRange = 400f; // pixels
    private float _speed;
    // Pause behavior: occasionally stop for a short duration
    private bool _isPaused = false;
    private float _pauseTimer = 0f;
    private float _pauseCooldown = 0f; // time until next pause
    private Random _rand = new Random();
    // Random movement direction (cardinal)
    private Vector2 _moveDirection = Vector2.Zero;
    private float _directionTimer = 0f;
    private const float DirectionChangeIntervalMin = 1.0f;
    private const float DirectionChangeIntervalMax = 3.5f;


    public EnemyShooter(Vector2 position, Art art, string name) : base(position, art, name)
    {
        // slower movement so side-spawn entry looks deliberate
        _speed = 1.5f;

        _shootTimer = (float)_rand.NextDouble() * ShootInterval;

        _pauseCooldown = (float)_rand.NextDouble() * 3f;

        UpdateTexture();
    }

    public override void Update()
    {
        // If this enemy is performing an entry movement, handle it and skip AI until done
        if (HandleEntry())
        {
            ClampToScreenBounds();
            return;
        }

        // choose a new random cardinal direction periodically
        _directionTimer -= ServiceLocator.DeltaSeconds;
        if (_directionTimer <= 0f || _moveDirection == Vector2.Zero)
        {
            int d = _rand.Next(4);
            switch (d)
            {
                case 0: _moveDirection = new Vector2(-1, 0); _currentDirection = Directions.Left; break;
                case 1: _moveDirection = new Vector2(1, 0); _currentDirection = Directions.Right; break;
                case 2: _moveDirection = new Vector2(0, -1); _currentDirection = Directions.Up; break;
                default: _moveDirection = new Vector2(0, 1); _currentDirection = Directions.Down; break;
            }
            _directionTimer = DirectionChangeIntervalMin + (float)_rand.NextDouble() * (DirectionChangeIntervalMax - DirectionChangeIntervalMin);
        }

        // Pause handling
        if (_isPaused)
        {
            _pauseTimer -= ServiceLocator.DeltaSeconds;
            if (_pauseTimer <= 0f)
            {
                _isPaused = false;
                _pauseCooldown = 1.5f + (float)_rand.NextDouble() * 3.0f;
            }
        }
        else
        {
            _pauseCooldown -= ServiceLocator.DeltaSeconds;
            if (_pauseCooldown <= 0f)
            {
                _isPaused = true;
                _pauseTimer = 1.0f;
            }
        }

        if (!_isPaused)
        {
            Vector2 startPos = _position;
            Vector2 velocity = _moveDirection * _speed;

            if (_scene != null)
            {
                if (_moveDirection.X != 0)
                {
                    Vector2 vx = new Vector2(velocity.X, 0);
                    vx = _scene.CheckForGridCollision(this, vx);
                    if (vx.X != 0)
                    {
                        Translate(new Vector2(vx.X, 0));
                    }
                    else
                    {
                        // blocked: choose new direction next update
                        _directionTimer = 0f;
                    }
                }
                else if (_moveDirection.Y != 0)
                {
                    Vector2 vy = new Vector2(0, velocity.Y);
                    vy = _scene.CheckForGridCollision(this, vy);
                    if (vy.Y != 0)
                    {
                        Translate(new Vector2(0, vy.Y));
                    }
                    else
                    {
                        _directionTimer = 0f;
                    }
                }
            }
            else
            {
                Translate(velocity);
            }

            // Update animation if moved
            Vector2 movedVec = _position - startPos;
            float movedDist = Math.Abs(movedVec.X) + Math.Abs(movedVec.Y);
            if (movedDist > 0.01f)
            {
                _animDistance += movedDist;
                if (_animDistance >= AnimStep)
                {
                    _animDistance -= AnimStep;
                    _animFrame = (_animFrame + 1) % 2;
                    UpdateTexture();
                }
            }
        }

        // Shooting: fire in the facing direction at intervals
        _shootTimer -= ServiceLocator.DeltaSeconds;
        if (_shootTimer <= 0f)
        {
            ShootInFacingDirection();
            _shootTimer = ShootInterval;
        }

        ClampToScreenBounds();

        // Check for collision with player
        if (_scene != null)
        {
            var hit = _scene.WhichEntityColliding(this, "player");
            if (hit is Player p)
            {
                p.TakeDamage(1);
            }
        }
    }
    private void ShootInFacingDirection()
    {
        Vector2 dir = Vector2.Zero;
        Vector2 bulletOffset = new Vector2();
        switch (_currentDirection)
        {
            case Directions.Left:
                dir = new Vector2(-1, 0);
                bulletOffset = new Vector2(-Grid.TileSize / 2, 12);
                break;
            case Directions.Right:
                dir = new Vector2(1, 0);
                bulletOffset = new Vector2(Grid.TileSize / 2, 12);
                break;
            case Directions.Up:
                dir = new Vector2(0, -1);
                bulletOffset = new Vector2(20, -Grid.TileSize / 2);
                break;
            case Directions.Down:
                dir = new Vector2(0, 1);
                bulletOffset = new Vector2(20, Grid.TileSize / 2);
                break;
        }
        if (dir == Vector2.Zero) return;

        Vector2 bulletPos = _position + bulletOffset;
        Bullet bullet = new Bullet(bulletPos, dir, BulletSpeed);
        if (_scene != null)
        {
            _scene.AddEntity(bullet);
        }
    }

    private void UpdateTexture()
    {
        if (AssetManager.EnemyShooterAnimations.TryGetValue(_currentDirection, out Texture2D[] frames))
        {
            if (frames.Length > 0)
            {
                int idx = Math.Clamp(_animFrame, 0, frames.Length - 1);
                _texture = frames[idx];
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}