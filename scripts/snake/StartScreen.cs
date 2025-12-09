using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Template;

/// <summary>
/// Stub class for lose screen - customize or remove for your game
/// </summary>
public class StartScreen : GameScreen
{
    public override void Load()
    {
        // Load lose screen resources

    }

    public override void Update(GameTime gameTime)
    {
        // MouseState mouse = Mouse.GetState();
        KeyboardState kb = Keyboard.GetState();

        // bool startClicked = mouse.LeftButton == ButtonState.Pressed;
        // bool startKey = kb.IsKeyDown(Keys.Enter) || kb.IsKeyDown(Keys.Space);

        bool startNormal = kb.IsKeyDown(Keys.D1);
        bool startHard = kb.IsKeyDown(Keys.D2);
        if (startNormal)
        {
            SnakeScene.level = levelEnum.normal;
            ScreenManager.ChangeScreen(new SnakeScene());
        }
        else if (startHard)
        {
            SnakeScene.level = levelEnum.hard;
            ScreenManager.ChangeScreen(new SnakeScene());
        }

        // if (startClicked || startKey)
        // {
        //     ScreenManager.ChangeScreen(new SnakeScene());
        // }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Color backgroundColor = Game1.ColorBlack;
        spriteBatch.GraphicsDevice.Clear(backgroundColor);
        spriteBatch.DrawString(
            AssetManager.font,
            "Snake Game",
            new Vector2(100, 100),
            Game1.ColorWhite
        );
        spriteBatch.DrawString(
            AssetManager.font,
            "Click 1 for normal difficulty press 2 for hard difficulty.",
            new Vector2(100, 140),
            Game1.ColorWhite
        );
    }
}
