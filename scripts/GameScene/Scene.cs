using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Template;

public class Scene : GameScreen
{
    private List<Entity> _entities = new List<Entity>();
    private List<Entity> _deadList = new List<Entity>();

    private Grid _grid;

    // HUD height offset
    public static readonly Vector2 SceneOffset = new Vector2(0, 224);
    public static readonly Vector2 GameSceneSize = new Vector2(256 * 4, 176 * 4);

    private TextEntity _entityCounterDisplay;

    public Grid Grid => _grid;
    public bool finished = false;
    public string nextScene = "";

    public Scene()
    {
    }

    private readonly string _levelName;
    public string LevelName => _levelName;

    public Scene(string levelName)
    {
        _levelName = levelName;

        // Initialize empty grid (CSV loading commented out - easy to re-enable)
        _grid = new Grid();
        // Uncomment to load from CSV:
        // LoadLevel(levelName);
        // _grid.Offset = SceneOffset;

        // Add HUD display
        AddEntity(new HUDDisplay(Vector2.Zero, ServiceLocator.GameState.HUD));

        // Add player at center of screen (no edge transition)
        Vector2 centerPos = new Vector2(
            GameSceneSize.X / 2 - Grid.TileSize / 2,
            SceneOffset.Y + GameSceneSize.Y / 2 - Grid.TileSize / 2
        );
        AddEntity(new Player(centerPos, Art.Player));

        // Spawn enemies directly in this screen
        SpawnEnemiesInScreen();
    }

    #region spawn enemies
    // Simple enemy spawning in current screen
    void SpawnEnemiesInScreen()
    {
        // Spawn 3-5 enemies at random positions
        Random rand = new Random();
        int enemyCount = rand.Next(3, 6);

        for (int i = 0; i < enemyCount; i++)
        {
            float x = rand.Next(100, (int)GameSceneSize.X - 100);
            float y = rand.Next((int)SceneOffset.Y + 100, (int)(SceneOffset.Y + GameSceneSize.Y - 100));
            Vector2 pos = new Vector2(x, y);

            // Randomly choose enemy type (33% each)
            int enemyType = rand.Next(3);
            if (enemyType == 0)
            {
                AddEntity(new EnemySpider(pos, Art.Enemy, "spider"));
            }
            else if (enemyType == 1)
            {
                AddEntity(new EnemyShooter(pos, Art.Enemy, "shooter"));
            }
            else
            {
                AddEntity(new EnemyChaser(pos, Art.Enemy, "chaser"));
            }
        }

        // Spawn 5-10 coins at random positions
        int coinCount = rand.Next(5, 11);
        for (int i = 0; i < coinCount; i++)
        {
            float x = rand.Next(50, (int)GameSceneSize.X - 50);
            float y = rand.Next((int)SceneOffset.Y + 50, (int)(SceneOffset.Y + GameSceneSize.Y - 50));
            Vector2 pos = new Vector2(x, y);
            AddEntity(new Coin(pos, Art.Coin));
        }
    }
    #endregion

    #region Level Loading (Commented out - Uncomment to enable CSV grid loading)
    /*
    void LoadLevel(string levelName)
    {
        string filePath = "Data/GridProject - " + levelName + ".csv";
        filePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        Console.WriteLine(filePath);
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            ParseLevelText(content);
        }
        else
        {
            throw new FileNotFoundException("Level file not found: " + filePath);
        }
    }
    
    void ParseLevelText(string levelContent)
    {
        _grid = new Grid();
        string[] lines = levelContent.Split('\n');
        for (int y = 0; y < lines.Length; y++)
        {
            string[] line = lines[y].Split(',');
            for (int x = 0; x < line.Length; x++)
            {
                string textData = line[x].Trim();
                Point p = new Point(x, y);
                if (textData == string.Empty) continue;
                bool entitySpawned = CheckForSpawnEntity(p, textData);
                if (entitySpawned)
                {
                    textData = string.Empty;
                }
                if (Tile.tileSymbols.ContainsKey(textData))
                {
                    _grid.SetTile(p, Tile.tileSymbols[textData]);
                }

            }
        }
    }
    */
    #endregion

    #region Edge Transition & Entity Spawning (Commented out - for CSV-based scene switching)
    /*
    void EdgeTransition()
    {
        if (ServiceLocator.GameState.EdgeTransitionPending && ServiceLocator.GameState.EdgeSpawnSide.HasValue)
        {
            AddEntity(new Player(ServiceLocator.GameState.PlayerPositionBeforeTransition, Art.Player));

            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i] is Player _player)
                {
                    Vector2 newPos = Vector2.Zero;
                    switch (ServiceLocator.GameState.EdgeSpawnSide.Value)
                    {
                        case GameState.SpawnEdge.Left:
                            newPos = new Vector2(10, ServiceLocator.GameState.PlayerPositionBeforeTransition.Y);
                            break;
                        case GameState.SpawnEdge.Right:
                            newPos = new Vector2(GameSceneSize.X - _player.Rect.Width - 10, ServiceLocator.GameState.PlayerPositionBeforeTransition.Y);
                            break;
                        case GameState.SpawnEdge.Top:
                            newPos = new Vector2(ServiceLocator.GameState.PlayerPositionBeforeTransition.X, SceneOffset.Y + 10);
                            break;
                        case GameState.SpawnEdge.Bottom:
                            newPos = new Vector2(ServiceLocator.GameState.PlayerPositionBeforeTransition.X, SceneOffset.Y + GameSceneSize.Y - _player.Rect.Height - 10);
                            break;
                        case GameState.SpawnEdge.FromDoor:
                            newPos = new Vector2(
                                GameSceneSize.X / 2 - _player.Rect.Width / 2,
                                SceneOffset.Y + GameSceneSize.Y - _player.Rect.Height - 10
                            );
                            break;
                        case GameState.SpawnEdge.Door:
                            newPos = new Vector2(
                                SceneOffset.X + Grid.TileSize * 4, SceneOffset.Y + Grid.TileSize * 2
                            );
                            break;
                    }
                    _player.Translate(newPos - _player.Position);
                    break;
                }
            }
            ServiceLocator.GameState.ClearEdgeTransition();
        }
    }

    bool CheckForSpawnEntity(Point gridPosition, string symbol)
    {
        string[] symbol_split = symbol.Split('_');
        Vector2 pixelPosition = Grid.GetPixelPositionFromGridPosition(gridPosition) + SceneOffset;
        switch (symbol_split[0])
        {
            case "player":
                if (!ServiceLocator.GameState.EdgeTransitionPending)
                {
                    AddEntity(new Player(pixelPosition, Art.Player));
                }
                return true;
            case "cave":
                AddEntity(new Door(pixelPosition, Art.Cave, symbol_split.Length >= 2 ? symbol_split[1] : ""));
                return true;
        }
        return false;
    }
    */
    #endregion

    public override void Update(GameTime gameTime)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity entity = _entities[i];
            // Console.WriteLine(entity.Label);
            entity.Update();
            if (entity.isDead)
            {
                _deadList.Add(entity);
            }
        }
        for (int i = 0; i < _deadList.Count; i++)
        {
            Entity entity = _deadList[i];
            RemoveEntity(entity);
        }
        _deadList.Clear();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawWorld(spriteBatch);
        DrawUI(spriteBatch);
    }

    // Draw the world (tiles + non-UI entities)
    public void DrawWorld(SpriteBatch spriteBatch)
    {
        if (_levelName == "Underworld")
        {
            Rectangle background = new Rectangle((int)SceneOffset.X, (int)SceneOffset.Y, (int)GameSceneSize.X, (int)GameSceneSize.Y);
            spriteBatch.Draw(AssetManager.GetTexture(Art.Pixel), background, Game1.ColorBlack);
        }

        _grid.Draw(spriteBatch);

        // Draw non-UI entities first
        for (int i = 0; i < _entities.Count; i++)
        {
            var e = _entities[i];
            if (e != null && e.Label != "ui")
            {
                e.Draw(spriteBatch);
            }
        }
    }

    // Draw UI elements (HUD etc.)
    public void DrawUI(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            var e = _entities[i];
            if (e != null && e.Label == "ui")
            {
                e.Draw(spriteBatch);
            }
        }
    }
    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
        entity.AddToScene(this);
    }
    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity);
        entity.RemoveFromScene(this);
    }

    /// <summary>
    /// Find and return the player entity in the scene
    /// </summary>
    public Player GetPlayer()
    {
        foreach (var entity in _entities)
        {
            if (entity is Player player)
                return player;
        }
        return null;
    }
    public void Open()
    {
        finished = false;
        nextScene = "";
    }
    public void Close()
    {
        finished = false;
        nextScene = "";
    }

    public void ChangeScene(string _nextScene)
    {
        finished = true;
        nextScene = _nextScene;
    }

    #region Collision Detection

    public bool IsColliding(Entity entity, string label = "")
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity other_entity = _entities[i];
            if (other_entity == entity) continue;

            if (!string.IsNullOrEmpty(label) && other_entity.Label != label) continue;
            if (entity.Rect.Intersects(other_entity.Rect))
            {
                if (label == "")
                {
                    return true;
                }
                else
                {
                    return label == other_entity.Label;
                }
            }
        }
        return false;
    }
    public Entity WhichEntityColliding(Entity entity, string label = "")
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity other_entity = _entities[i];
            if (other_entity == entity) continue;
            if (entity.Rect.Intersects(other_entity.Rect))
            {
                if (label == "")
                {
                    return other_entity;
                }
                else
                {
                    if (label == other_entity.Label)
                    {
                        return other_entity;
                    }
                }
            }
        }
        return null;
    }

    public Entity WhichEntityColliding(Entity entity, Rectangle rect, string label = "")
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            Entity other_entity = _entities[i];
            if (other_entity == entity) continue;

            if (rect.Intersects(other_entity.Rect))
            {
                if (label == "")
                {
                    return other_entity;
                }
                else
                {
                    if (label == other_entity.Label)
                    {
                        return other_entity;
                    }
                }
            }
        }
        return null;
    }

    public Vector2 CheckForGridCollision(Entity entity, Vector2 velocity)
    {
        Rectangle nextRect = new Rectangle(
            (int)(entity.Rect.X + velocity.X),
            (int)(entity.Rect.Y + velocity.Y),
            entity.Rect.Width,
            entity.Rect.Height
        );

        bool topRightSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(nextRect.Right - 1, nextRect.Top));
        bool topLeftSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(nextRect.Left, nextRect.Top));
        bool bottomRightSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(nextRect.Right - 1, nextRect.Bottom - 1));
        bool bottomLeftSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(nextRect.Left, nextRect.Bottom - 1));

        if (velocity.X > 0)
        {
            if (topRightSolid || bottomRightSolid)
            {
                velocity.X = 0;
            }
        }
        else if (velocity.X < 0)
        {
            if (topLeftSolid || bottomLeftSolid)
            {
                velocity.X = 0;
            }
        }
        if (velocity.Y > 0)
        {
            if (bottomRightSolid || bottomLeftSolid)
            {
                velocity.Y = 0;
            }
        }
        else if (velocity.Y < 0)
        {
            if (topRightSolid || topLeftSolid)
            {
                velocity.Y = 0;
            }
        }

        return velocity;
    }

    public bool HasGridCollision(Entity entity)
    {
        Rectangle rect = entity.Rect;

        bool topRightSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Right - 1, rect.Top));
        bool topLeftSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Left, rect.Top));
        bool bottomRightSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Right - 1, rect.Bottom - 1));
        bool bottomLeftSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Left, rect.Bottom - 1));

        return topRightSolid || topLeftSolid || bottomRightSolid || bottomLeftSolid;
    }

    public bool HasGridCollisionAt(Rectangle rect)
    {
        bool topRightSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Right - 1, rect.Top));
        bool topLeftSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Left, rect.Top));
        bool bottomRightSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Right - 1, rect.Bottom - 1));
        bool bottomLeftSolid = _grid.IsPixelSolidByBackgroundColor(new Vector2(rect.Left, rect.Bottom - 1));

        return topRightSolid || topLeftSolid || bottomRightSolid || bottomLeftSolid;
    }
    #endregion

}