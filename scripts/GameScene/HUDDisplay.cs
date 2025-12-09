using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

/// <summary>
/// Simple text-based HUD showing player stats
/// </summary>
public class HUDDisplay : Entity
{
    private SpriteFont _font;
    private Vector2 _position;

    public HUDDisplay(Vector2 position, object hud) : base(position)
    {
        _position = position;
        _font = AssetManager.font;
        _label = "ui";
    }

    public override void Update()
    {
        // Update HUD logic if needed
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_font == null || ServiceLocator.GameState == null) return;

        Player player = ServiceLocator.GameState.Player;
        if (player == null) return;

        // Draw health
        string healthText = $"Health: {player.Health}/{player.MaxHealth}";
        spriteBatch.DrawString(_font, healthText, new Vector2(20, 20), Color.Red);

        // Draw attack
        string attackText = $"Attack: {player.Attack}";
        spriteBatch.DrawString(_font, attackText, new Vector2(20, 50), Color.White);

        // Draw sword status
        if (ServiceLocator.GameState.HasSword)
        {
            spriteBatch.DrawString(_font, "Sword: Yes (Z)", new Vector2(20, 80), Color.Black);
        }

        // Draw controls hint
        spriteBatch.DrawString(_font, "Move: Arrows/Left Click | Shoot: Right Click/X",
            new Vector2(20, Game1.ScreenSize.Y - 40), Color.Blue);        // Optional: Draw custom state values
        // Example: int score = ServiceLocator.GameState.GetState<int>("score", 0);
        // spriteBatch.DrawString(_font, $"Score: {score}", new Vector2(20, 110), Color.White);
    }
}
