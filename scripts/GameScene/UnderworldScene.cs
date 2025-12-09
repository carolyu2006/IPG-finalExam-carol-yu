using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

// UnderworldScene wraps a normal Scene but shows an intro text animation
// and temporarily disables player control until the sword is spawned.
public class UnderworldScene : GameScreen
{
    private Scene _scene;
    private TextEntity _textEntity;
    private string _fullText = "IT'S DANGEROUS TO GO ALONE! TAKE THIS.";
    private int _textIndex = 0;
    private float _timer = 0f;
    private const float CharInterval = 0.04f; // seconds per character
    private bool _textFinished = false;
    private bool _swordSpawned = false;

    private string _sceneName;


    public UnderworldScene(string sceneName)
    {
        _sceneName = sceneName;
    }

    public override void Load()
    {
        _scene = new Scene(_sceneName);
        _scene.Open();

        Vector2 spawnPos = new Vector2(Scene.GameSceneSize.X / 2 - Grid.TileSize / 2, Scene.SceneOffset.Y + Scene.GameSceneSize.Y / 2 - Grid.TileSize / 2);
        var spawnGrid = Grid.GetGridPositionFromPixelPosition(spawnPos);

        bool alreadyCollected = false;
        if (ServiceLocator.GameState != null)
        {
            alreadyCollected = ServiceLocator.GameState.IsItemCollected(_sceneName, spawnGrid);
        }

        if (alreadyCollected)
        {
            _textFinished = true;
            _swordSpawned = true;
            if (ServiceLocator.GameState != null)
                ServiceLocator.GameState.AllowPlayerControl = true;
        }
        else
        {
            if (ServiceLocator.GameState != null)
                ServiceLocator.GameState.AllowPlayerControl = false;
            Vector2 textPos = new Vector2(Scene.GameSceneSize.X / 2 - 200, Scene.SceneOffset.Y + 200);
            _textEntity = new TextEntity(textPos, "");
            _scene.AddEntity(_textEntity);
        }

        // Always add the decorative fire animations to the scene (looping)
        try
        {
            var leftFirePos = new Vector2(300, Scene.SceneOffset.Y + (int)(64 * 4));
            var rightFirePos = new Vector2(Game1.ScreenSize.X - 300 - 64, Scene.SceneOffset.Y + (int)(64 * 4));
            if (AssetManager.fireAnimationFrames != null && AssetManager.fireAnimationFrames.Length > 0)
            {
                _scene.AddEntity(new AnimationEntity(leftFirePos, AssetManager.fireAnimationFrames, 0.15f, true));
                _scene.AddEntity(new AnimationEntity(rightFirePos, AssetManager.fireAnimationFrames, 0.15f, true));
                _scene.AddEntity(new SpriteEntity(new Vector2(Scene.GameSceneSize.X / 2 - Grid.TileSize / 2, Scene.SceneOffset.Y + (int)(64 * 4)), Art.NPCLarge));
            }
        }
        catch (Exception)
        {
            // ignore if assets not available
        }
    }

    public override void Update(GameTime gameTime)
    {
        _scene.Update(gameTime);

        if (_scene.finished)
        {
            string next = _scene.nextScene;
            _scene.Close();
            if (ServiceLocator.GameState != null)
                ServiceLocator.GameState.AllowPlayerControl = true;

            if (!string.IsNullOrEmpty(next))
            {
                ScreenManager.ChangeScreen(new SceneScreen(next));
                return;
            }
        }

        if (!_textFinished)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (_timer >= CharInterval && _textIndex < _fullText.Length)
            {
                _timer -= CharInterval;
                _textIndex++;
                _textEntity.SetText(_fullText.Substring(0, _textIndex));
            }

            if (_textIndex >= _fullText.Length)
            {
                _textFinished = true;
                _timer = 0f;
            }
        }
        else if (!_swordSpawned)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer > 0.6f)
            {
                SpawnSword();
                _swordSpawned = true;
                if (ServiceLocator.GameState != null)
                    ServiceLocator.GameState.AllowPlayerControl = true;
                _scene.RemoveEntity(_textEntity);
            }
        }

    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _scene.Draw(spriteBatch);
        // Decorative fire animations are now entities inside the scene
    }

    void SpawnSword()
    {
        if (_scene == null) return;

        Vector2 pos = new Vector2(Scene.GameSceneSize.X / 2 - Grid.TileSize / 2, Scene.SceneOffset.Y + (int)(64 * 5.5));

        Item swordItem = Item.Sword;
        Art art = Art.sword;

        ItemPickUp pickup = new ItemPickUp(pos, art, swordItem);
        _scene.AddEntity(pickup);
    }
}