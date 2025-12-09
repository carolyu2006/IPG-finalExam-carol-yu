using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Template
{
    public class Game1 : Game
    {
        // Debug mode toggle
        public static bool Debug = true;

        // Screen configuration
        public static Vector2 ScreenSize = new Vector2(256 * 4, 232 * 4);

        // Color palette - customize as needed
        public static Color ColorBackground = new Color(252, 216, 168);
        public static Color ColorWhite = new Color(247, 243, 242);
        public static Color ColorBlack = new Color(0, 0, 0);

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Save/Load key bindings
        private KeyboardState _previousKeyState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialize service locator
            ServiceLocator.Game1 = this;
            ServiceLocator.GameState = new GameState();
            ServiceLocator.Input = new Input();

            // Set screen size
            _graphics.PreferredBackBufferWidth = (int)ScreenSize.X;
            _graphics.PreferredBackBufferHeight = (int)ScreenSize.Y;
            _graphics.ApplyChanges();

            // Load all game assets
            AssetManager.LoadContent(Content);

            // Load enemy stats from JSON
            Enemy.LoadEnemyStatsFromJson("Data/enemy.json");

            // Start with default scene - customize this
            // ScreenManager.ChangeScreen(new SceneScreen("Zelda12"));
            ScreenManager.ChangeScreen(new StartScreen());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit game
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentKeyState = Keyboard.GetState();

            // Save game - Press 1
            if (currentKeyState.IsKeyDown(Keys.D1) && _previousKeyState.IsKeyUp(Keys.D1))
            {
                ServiceLocator.GameState.SaveToFile("savegame.json");
            }

            // Load game - Press 2
            if (currentKeyState.IsKeyDown(Keys.D2) && _previousKeyState.IsKeyUp(Keys.D2))
            {
                if (ServiceLocator.GameState.LoadFromFile("savegame.json"))
                {
                    // Optionally reload scene after loading
                    string sceneName = ServiceLocator.GameState.CurrentSceneName;
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        ScreenManager.ChangeScreen(new SceneScreen(sceneName));
                    }
                }
            }

            _previousKeyState = currentKeyState;

            // Update input and game systems
            Input.Update();
            ServiceLocator.DeltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ColorBackground);

            // Use point clamp for pixel-perfect rendering
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            _spriteBatch.Begin();
            ScreenManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
