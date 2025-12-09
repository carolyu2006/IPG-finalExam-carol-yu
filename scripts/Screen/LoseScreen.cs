using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

/// <summary>
/// Stub class for lose screen - customize or remove for your game
/// </summary>
public class LoseScreen : GameScreen
{
    public override void Load()
    {
        // Load lose screen resources

    }

    public override void Update(GameTime gameTime)
    {
        // Update lose screen logic
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Color backgroundColor = Game1.ColorBlack;

        // spriteBatch.Begin();
        spriteBatch.GraphicsDevice.Clear(backgroundColor);

        spriteBatch.DrawString(
            AssetManager.font,
            "You Lose!",
            new Vector2(100, 100),
            Game1.ColorWhite
        );
    }
}
