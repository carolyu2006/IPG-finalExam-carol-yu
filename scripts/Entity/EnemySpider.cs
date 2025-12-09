using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Template;

public enum SpiderState
{
    Up,
    Down
}

public class EnemySpider : Enemy
{
    private SpiderState _state = SpiderState.Down;

    private Vector2 _moveDirection = Vector2.Zero;
    // Jumping behaviour fields
    private bool _isJumping = false;
    private float _jumpTimer = 0f;
    private float _jumpDuration = 0.3f; // seconds
    private Vector2 _jumpStart = Vector2.Zero;
    private Vector2 _jumpTarget = Vector2.Zero;
    private float _jumpHeight = 26f; // peak height in pixels
    private float _nextJumpTimer = 1.5f; // time until next jump when landed

    // slower walker speed (used minimally when not jumping)
    private Random _rand = new Random();

    public EnemySpider(Vector2 position, Art art, string name) : base(position, art, name)
    {
        // Start with random direction (unused for major movement) and schedule first jump
        ChooseRandomDirection();
        _nextJumpTimer = 0.5f + (float)_rand.NextDouble() * 2.0f;
        UpdateTexture();
    }

    public override void Update()
    {
        // If performing entry movement, handle it first and skip AI until complete
        if (HandleEntry())
        {
            ClampToScreenBounds();
            return;
        }

        // Jumping behaviour: when landed (Down), count down to next jump; when jumping, animate arc
        if (_isJumping)
        {
            _jumpTimer += ServiceLocator.DeltaSeconds;
            float t = Math.Clamp(_jumpTimer / _jumpDuration, 0f, 1f);

            // horizontal interpolation
            Vector2 horiz = Vector2.Lerp(_jumpStart, _jumpTarget, t);
            // vertical parabolic arc: peak at t=0.5 -> offset = 4*h*t*(1-t)
            float arc = 4f * _jumpHeight * t * (1f - t);
            _position = new Vector2(horiz.X, horiz.Y - arc);

            // ensure Up frame while jumping
            _state = SpiderState.Up;
            UpdateTexture();

            if (t >= 1f)
            {
                // Landed
                _isJumping = false;
                _position = _jumpTarget;
                _state = SpiderState.Down;
                UpdateTexture();
                ClampToScreenBounds();
                // schedule next jump
                _nextJumpTimer = 0.8f + (float)_rand.NextDouble() * 2.0f;
            }
        }
        else
        {
            // landed — countdown to next jump
            _nextJumpTimer -= ServiceLocator.DeltaSeconds;
            if (_nextJumpTimer <= 0f)
            {
                StartJump();
            }
        }

        // Ensure we remain in playable area when not jumping
        if (!_isJumping)
        {
            ClampToScreenBounds();
        }

        // Check for collision with player
        if (_scene != null)
        {
            var hit = _scene.WhichEntityColliding(this, "player");
            if (hit is Player player)
            {
                // Damage the player on contact
                player.TakeDamage(1);
            }
        }

        // Don't call base.Update() since we're overriding all behavior
    }

    private void ChooseRandomDirection()
    {
        int dir = _rand.Next(4);
        switch (dir)
        {
            case 0: _moveDirection = new Vector2(-1, 0); break; // left
            case 1: _moveDirection = new Vector2(1, 0); break;  // right
            case 2: _moveDirection = new Vector2(0, -1); break; // up
            case 3: _moveDirection = new Vector2(0, 1); break;  // down
        }
    }

    private void StartJump()
    {
        _isJumping = true;
        _jumpTimer = 0f;
        _jumpDuration = (float)_rand.NextDouble() * 0.6f;
        _jumpHeight = 20f + (float)_rand.NextDouble() * 18f;
        _jumpStart = _position;

        // choose a random horizontal offset in tiles (-3..3)
        int offsetTiles = _rand.Next(-3, 4);
        float targetX = _position.X + offsetTiles * Grid.TileSize;
        float targetY = _position.Y; // same vertical ground level

        // Clamp target to scene/playable area if available
        if (_scene != null)
        {
            float minX = Scene.SceneOffset.X;
            float maxX = Scene.SceneOffset.X + Scene.GameSceneSize.X - Grid.TileSize;
            targetX = Math.Clamp(targetX, minX, maxX);
        }

        _jumpTarget = new Vector2(targetX, targetY);
        _state = SpiderState.Up;
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        if (AssetManager.EnemySpiderArt.TryGetValue(
            _state == SpiderState.Down ? Directions.Down : Directions.Up,
            out Texture2D frame))
        {
            _texture = frame;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}