using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

// A screen that hosts a Scene (level)
public class SceneScreen : GameScreen
{
    private string _levelName;
    private Scene _scene;

    public SceneScreen(string levelName)
    {
        _levelName = levelName;
    }

    public SceneScreen(Scene scene)
    {
        _scene = scene;
        _levelName = scene?.LevelName;
    }

    public override void Load()
    {
        // If a Scene instance was provided (prepared by a transition), use it directly.
        if (_scene != null)
        {
            // If this level is the special Underworld, switch to the UnderworldScene wrapper
            if (_scene.LevelName == "Underworld")
            {
                ScreenManager.ChangeScreen(new UnderworldScene(_scene.LevelName));
                return;
            }
            _scene.Open();
            return;
        }

        // Otherwise create a fresh scene from the level name.
        // If this level is the special Underworld, switch to the UnderworldScene wrapper
        if (_levelName == "Underworld")
        {
            ScreenManager.ChangeScreen(new UnderworldScene(_levelName));
            return;
        }

        _scene = new Scene(_levelName);
        _scene.Open();
    }

    public override void Update(GameTime gameTime)
    {
        _scene.Update(gameTime);
        if (_scene.finished)
        {
            var next = _scene.nextScene;
            if (!string.IsNullOrEmpty(next))
            {
                if (next == "Underworld")
                {
                    ScreenManager.ChangeScreen(new SceneScreen(next));
                    return;
                }

                ScreenManager.ChangeScreen(new SceneScreen(next));
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _scene.Draw(spriteBatch);
    }

    public override void Unload()
    {
        // If you add disposable resources to Scene, release them here.
    }
}
