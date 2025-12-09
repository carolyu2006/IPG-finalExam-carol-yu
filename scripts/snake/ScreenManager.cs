using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

/// <summary>
/// Simple screen manager for handling game screens
/// </summary>
public static class ScreenManager
{
    private static GameScreen _current;

    // Legacy property for backwards compatibility
    public static Vector2 TransitionOffset = Vector2.Zero;

    public static void ChangeScreen(GameScreen screen)
    {
        _current = screen;
        _current.Load();
    }

    public static void Update(GameTime gameTime)
    {
        _current?.Update(gameTime);
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        _current?.Draw(spriteBatch);
    }
}
