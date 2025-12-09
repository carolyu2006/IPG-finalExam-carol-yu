using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

// Base enemy with simple chase AI and collision handling.
// Derive from this class to create specific enemy behaviors.
public class Enemy : SpriteEntity
{
    #region fields and properties
    // Static dictionary to cache loaded enemy stats from JSON
    private static Dictionary<string, EnemyStats> _enemyStatsCache = new Dictionary<string, EnemyStats>();
    private static bool _statsLoaded = false;

    // Entry movement when spawned at edge: move inward for a short distance
    protected Vector2 _entryDirection = Vector2.Zero;
    protected float _entryRemaining = 0f; // pixels remaining to move
    protected float _entrySpeed = 0f; // pixels per update
    #endregion


    // Start an entry movement: inwardDirection should be a normalized cardinal vector
    public void StartEntry(Vector2 inwardDirection, float distancePixels, float speedPerUpdate)
    {
        _entryDirection = inwardDirection;
        _entryRemaining = Math.Abs(distancePixels);
        _entrySpeed = Math.Abs(speedPerUpdate);
    }

    // Handle entry movement; returns true if entry movement was active this frame
    protected bool HandleEntry()
    {
        if (_entryRemaining <= 0f) return false;

        float moveAmount = Math.Min(_entrySpeed, _entryRemaining);
        Vector2 moveVec = Vector2.Zero;
        if (Math.Abs(_entryDirection.X) > 0.1f)
        {
            moveVec = new Vector2(Math.Sign(_entryDirection.X) * moveAmount, 0);
        }
        else if (Math.Abs(_entryDirection.Y) > 0.1f)
        {
            moveVec = new Vector2(0, Math.Sign(_entryDirection.Y) * moveAmount);
        }

        if (_scene != null)
        {
            // respect grid collisions per-axis similar to other enemies
            if (moveVec.X != 0)
            {
                Vector2 vx = new Vector2(moveVec.X, 0);
                vx = _scene.CheckForGridCollision(this, vx);
                if (vx.X != 0) Translate(new Vector2(vx.X, 0));
            }
            if (moveVec.Y != 0)
            {
                Vector2 vy = new Vector2(0, moveVec.Y);
                vy = _scene.CheckForGridCollision(this, vy);
                if (vy.Y != 0) Translate(new Vector2(0, vy.Y));
            }
        }
        else
        {
            Translate(moveVec);
        }

        _entryRemaining -= Math.Abs(moveAmount);
        if (_entryRemaining <= 0f)
        {
            _entryDirection = Vector2.Zero;
            _entrySpeed = 0f;
            _entryRemaining = 0f;
        }

        return true;
    }

    public static Enemy MakeEnemy(Vector2 position, Art art, string name)
    {
        switch (name)
        {
            case "Spider":
                return new EnemySpider(position, art, name);
            case "Shooter":
                return new EnemyShooter(position, art, name);
            case "Chaser":
                return new EnemyChaser(position, art, name);
        }
        return new Enemy(position, art, name);
    }

    #region JSON Loading
    /// <summary>
    /// Load enemy stats from JSON file (Data/enemy.json)
    /// </summary>
    public static void LoadEnemyStatsFromJson(string filePath = "Data/enemy.json")
    {
        if (_statsLoaded) return;

        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("enemies", out JsonElement enemiesElement))
                    {
                        foreach (JsonProperty enemyProp in enemiesElement.EnumerateObject())
                        {
                            string enemyName = enemyProp.Name;
                            JsonElement statsElement = enemyProp.Value;

                            var stats = new EnemyStats
                            {
                                MaxHealth = statsElement.TryGetProperty("maxHealth", out var mh) ? mh.GetInt32() : 1,
                                Attack = statsElement.TryGetProperty("attack", out var att) ? att.GetInt32() : 1,
                                Speed = statsElement.TryGetProperty("speed", out var spd) ? (float)spd.GetDouble() : 2.0f,
                                Position = statsElement.TryGetProperty("position", out var pos) ?
                                    new Vector2(
                                        pos.TryGetProperty("x", out var px) ? px.GetSingle() : 0f,
                                        pos.TryGetProperty("y", out var py) ? py.GetSingle() : 0f
                                    ) : Vector2.Zero
                            };

                            _enemyStatsCache[enemyName] = stats;
                        }
                        _statsLoaded = true;
                        Console.WriteLine("[Enemy] Successfully loaded enemy stats from JSON");
                    }
                }
            }
            else
            {
                Console.WriteLine($"[Enemy] Warning: JSON file not found at {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Enemy] Error loading enemy stats from JSON: {ex.Message}");
        }
    }
    #endregion

    /// <summary>
    /// Get enemy stats from cache (must call LoadEnemyStatsFromJson first)
    /// </summary>
    public static EnemyStats GetEnemyStats(string enemyName)
    {
        if (_enemyStatsCache.TryGetValue(enemyName, out var stats))
        {
            return stats;
        }
        // Return default stats if not found
        return new EnemyStats { MaxHealth = 1, Attack = 1, Speed = 2.0f, Position = Vector2.Zero };
    }

    protected Player _playerRef;

    // Basic stats
    protected int _maxHealth = 1;
    protected int _health = 1;
    protected int _attack = 1;

    protected string _name;
    public string Name => _name;

    #region constructor
    public Enemy(Vector2 position, Art art, String name) : base(position, art)
    {
        _label = "enemy";
        _name = name;

        // OPTION 1: Load stats from JSON (new way)
        EnemyStats stats = GetEnemyStats(name);
        _maxHealth = stats.MaxHealth;
        _health = stats.MaxHealth;
        _attack = stats.Attack;
        _position = stats.Position;

        /* OPTION 2: Hardcoded stats (old way - commented out)
        switch (name)
        {
            case "Spider":
                _maxHealth = 1;
                _health = 1;
                _attack = 1;
                break;
            case "Shooter":
                _maxHealth = 2;
                _health = 2;
                _attack = 1;
                break;
            case "Chaser":
                _maxHealth = 1;
                _health = 1;
                _attack = 1;
                break;
            default:
                _maxHealth = 1;
                _health = 1;
                _attack = 1;
                break;
        }
        */
    }
    #endregion

    #region takeDamage and death
    public virtual void TakeDamage(int amount)
    {
        _health -= Math.Max(1, amount);
        if (_health <= 0)
        {
            _health = 0;
            Random probability = new Random();

            // Spawn death animation if available
            if (AssetManager.DeathFrames != null && AssetManager.DeathFrames.Length > 0)
            {
                AnimationEntity deathAnim = new AnimationEntity(_position, AssetManager.DeathFrames, 0.12f, false);
                _scene.AddEntity(deathAnim);
            }

            if (probability.NextDouble() < 0.5)
            {
                Item coinItem = Item.Coin;
                Art art = Art.Coin;

                ItemPickUp pickup = new ItemPickUp(_position, art, coinItem);
                _scene.AddEntity(pickup);
            }

            Die();
        }
        else
        {
            OnHit();
        }
    }
    #endregion

    public override void Update()
    {
        base.Update();
    }

    #region ClampToScreen
    protected virtual void ClampToScreenBounds()
    {
        if (_scene != null)
        {
            // Clamp to the scene's playable area (respect HUD offset)
            float minX = Scene.SceneOffset.X;
            float minY = Scene.SceneOffset.Y;
            float maxX = Scene.SceneOffset.X + Scene.GameSceneSize.X - _rect.Width;
            float maxY = Scene.SceneOffset.Y + Scene.GameSceneSize.Y - _rect.Height;

            if (_position.X < minX) _position.X = minX;
            if (_position.Y < minY) _position.Y = minY;
            if (_position.X > maxX) _position.X = maxX;
            if (_position.Y > maxY) _position.Y = maxY;
        }
        else
        {
            if (_position.X < 0) _position.X = 0;
            if (_position.Y < 0) _position.Y = 0;
            if (_position.X > Game1.ScreenSize.X - _rect.Width) _position.X = Game1.ScreenSize.X - _rect.Width;
            if (_position.Y > Game1.ScreenSize.Y - _rect.Height) _position.Y = Game1.ScreenSize.Y - _rect.Height;
        }

        _rect = new Rectangle((int)_position.X + _rect_offset.X, (int)_position.Y + _rect_offset.Y, Grid.TileSize - _rect_offset.X * 2, Grid.TileSize - _rect_offset.Y);
    }
    #endregion

    protected virtual void OnHit() { }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        // Optional: add simple debug visual for detection radius when debugging
        // if (Game1.Debug) { /* draw a circle or bounding box if desired */ }
    }
}


#region Data Structures
/// <summary>
/// Data structure to hold enemy statistics loaded from JSON
/// </summary>
public struct EnemyStats
{
    public int MaxHealth { get; set; }
    public int Attack { get; set; }
    public float Speed { get; set; }
    public Vector2 Position { get; set; }
}
#endregion
