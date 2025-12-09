using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Template;

/// <summary>
/// Coin collectible that gives the player points when picked up
/// </summary>
public class Coin : SpriteEntity
{
    private int _value = 1; // How many coins this pickup is worth
    private float _bobTimer = 0f; // For bobbing animation
    private float _bobSpeed = 2f; // Speed of bobbing
    private float _bobHeight = 4f; // Height of bobbing in pixels
    private Vector2 _basePosition; // Original position for bobbing calculation

    public int Value => _value;

    public Coin(Vector2 position, Art art, int value = 1) : base(position, art)
    {
        _label = "coin";
        _value = value;
        _basePosition = position;
        _rect_offset = new Point(8, 8);
        _rect = new Rectangle((int)position.X + _rect_offset.X, (int)position.Y + _rect_offset.Y,
            Grid.TileSize - _rect_offset.X * 2, Grid.TileSize - _rect_offset.Y * 2);
    }

    public override void Update()
    {
        base.Update();

        // Bobbing animation
        _bobTimer += ServiceLocator.DeltaSeconds;
        float bobOffset = MathF.Sin(_bobTimer * _bobSpeed) * _bobHeight;
        _position = _basePosition + new Vector2(0, bobOffset);

        // Update rectangle position
        _rect = new Rectangle((int)_position.X + _rect_offset.X, (int)_position.Y + _rect_offset.Y,
            Grid.TileSize - _rect_offset.X * 2, Grid.TileSize - _rect_offset.Y * 2);

        // Check for collision with player
        if (_scene != null)
        {
            var hit = _scene.WhichEntityColliding(this, "player");
            if (hit is Player player)
            {
                // Give coin to player
                int currentCoins = ServiceLocator.GameState.GetState<int>("Coins");
                ServiceLocator.GameState.SetState("Coins", currentCoins + _value);

                // Remove coin
                Die();
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}
