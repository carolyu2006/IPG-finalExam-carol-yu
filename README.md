# Legend of Zelda Game Template - Documentation

A flexible MonoGame template for creating 2D action-adventure games with entity management, scene systems, and save/load functionality.

---

## Table of Contents
- [Quick Start](#quick-start)
- [Core Systems](#core-systems)
- [File Structure Guide](#file-structure-guide)
- [How-To Guides](#how-to-guides)
- [API Reference](#api-reference)

---

## Quick Start

### Controls
- **Movement**: Arrow Keys or Left Click (mouse)
- **Sword Attack**: Z key (when you have sword)
- **Shoot Projectile**: Right Click or X key
- **Save Game**: 1
- **Load Game**: 2

### Running the Game
```bash
dotnet build
dotnet run
```

---

## Core Systems

### 1. GameState (Manager/GameState.cs)
**Purpose**: Central state management with JSON save/load functionality

**Key Methods**:
```csharp
// Store any value
SetState(string key, object value)

// Retrieve values with type safety
T GetState<T>(string key, T defaultValue = default)

// Check if key exists
bool HasState(string key)

// Save to JSON file
SaveToFile(string filename = "save.json")

// Load from JSON file
LoadFromFile(string filename = "save.json")
```

**Usage Example**:
```csharp
// Store values
ServiceLocator.GameState.SetState("Coins", 10);
ServiceLocator.GameState.SetState("Level", 5);
ServiceLocator.GameState.SetState("PlayerName", "Link");

// Retrieve values
int coins = ServiceLocator.GameState.GetState<int>("Coins");
string name = ServiceLocator.GameState.GetState<string>("PlayerName");

// Save/Load
ServiceLocator.GameState.SaveToFile("mysave.json");
ServiceLocator.GameState.LoadFromFile("mysave.json");
```

**Built-in Properties**:
- `Player` - Reference to player entity
- `PlayerHealth` / `PlayerMaxHealth` - Player health tracking
- `HasSword` - Sword possession flag
- `AllowPlayerControl` - Enable/disable player input
- `CurrentSceneName` - Current scene identifier

---

### 2. Input System (Manager/Input.cs)
**Purpose**: Unified keyboard and mouse input handling

**Keyboard Methods**:
```csharp
bool IsActionPressed(Action action)  // Pressed this frame
bool IsActionDown(Action action)     // Currently held down
```

**Mouse Methods**:
```csharp
static Vector2 GetMousePosition()           // Current mouse position
static bool IsLeftMouseClick()              // Left click this frame
static bool IsRightMouseClick()             // Right click this frame
static bool IsLeftMouseDown()               // Left button held
static bool IsRightMouseDown()              // Right button held
static bool IsMouseDragging()               // Mouse moving while held
static Vector2 GetMouseDragDelta()          // Drag distance
static bool IsMouseHovering()               // Mouse position changed
static Vector2 GetPreviousMousePosition()   // Last frame mouse position
```

**Action Enum**:
```csharp
Action.MoveLeft, Action.MoveRight, Action.MoveUp, Action.MoveDown
Action.Talk, Action.Start, Action.Select
Action.ActionA  // Z key (sword)
Action.ActionB  // X key (shoot)
```

**Usage Example**:
```csharp
// Keyboard
if (ServiceLocator.Input.IsActionPressed(Action.ActionA)) {
    // Swing sword
}

// Mouse
if (Input.IsLeftMouseClick()) {
    Vector2 clickPos = Input.GetMousePosition();
    // Move player to click position
}

if (Input.IsMouseDragging()) {
    Vector2 dragDelta = Input.GetMouseDragDelta();
    // Handle drag
}
```

---

### 3. Scene System (GameScene/Scene.cs)
**Purpose**: Game world container managing entities, collision, and level data

**Core Methods**:
```csharp
void AddEntity(Entity entity)                // Add entity to scene
void RemoveEntity(Entity entity)             // Remove entity
Player GetPlayer()                           // Find player in scene
Entity WhichEntityColliding(Entity e, string label)  // Collision detection
Vector2 CheckForGridCollision(Entity e, Vector2 velocity)  // Grid collision
void ChangeScene(string nextScene)           // Switch to different scene
```

**Usage Example**:
```csharp
// Add entities
scene.AddEntity(new EnemySpider(pos, Art.Enemy, "spider"));
scene.AddEntity(new Coin(pos, Art.Coin));

// Get player reference
Player player = _scene.GetPlayer();

// Check collision
var hit = _scene.WhichEntityColliding(this, "enemy");
if (hit is Enemy enemy) {
    enemy.TakeDamage(5);
}

// Scene transition
scene.ChangeScene("Zelda02");
```

**Properties**:
- `SceneOffset` - Static Vector2(0, 224) - HUD offset
- `GameSceneSize` - Static Vector2(1024, 704) - Playable area size
- `Grid` - Level grid reference

---

### 4. Screen Manager (Screen/ScreenManager.cs)
**Purpose**: Handle screen/scene switching

**Methods**:
```csharp
static void ChangeScreen(GameScreen screen)
static void Update(GameTime gameTime)
static void Draw(SpriteBatch spriteBatch)
```

**Usage Example**:
```csharp
// Switch to game scene
ScreenManager.ChangeScreen(new SceneScreen());

// Switch to custom screen
ScreenManager.ChangeScreen(new MyCustomScreen());
```

---

## File Structure Guide

### Entity Files (scripts/Entity/)

#### **Player.cs**
**Purpose**: Player character with movement, combat, and mouse control

**Key Methods**:
```csharp
void HandleInput()          // Process keyboard/mouse input
void Move(Vector2 velocity) // Apply movement with collision
void TakeDamage(int amount) // Receive damage
void Die()                  // Player death handling
```

**Properties**:
- `Health` / `MaxHealth` - Health values
- `Attack` - Attack damage
- `CurrentDirection` - Facing direction (Directions enum)

**How to Modify**:
```csharp
// Change movement speed (line ~35)
private float _speed = 150f; // pixels per second

// Change attack damage
player.Attack = 5;

// Add new abilities
public void UseSpecialAbility() {
    // Your code here
}
```

---

#### **Enemy.cs** (Base Class)
**Purpose**: Base enemy class with health, damage, and death handling

**Key Methods**:
```csharp
virtual void TakeDamage(int amount)     // Damage handling
virtual void Update()                    // Per-frame logic
static Enemy MakeEnemy(Vector2 pos, Art art, string name)  // Factory method
void StartEntry(Vector2 dir, float dist, float speed)  // Entry animation
```

**Derived Classes**:

##### **EnemySpider.cs**
- Jumping enemy with parabolic arc movement
- Jumps every 0.8-2.5 seconds
- Jump height: 26 pixels

##### **EnemyShooter.cs**
- Ranged enemy that shoots projectiles
- Walking movement between shots
- Shoots at player when in range

##### **EnemyChaser.cs**
- Actively chases player
- Detection range: 300 pixels
- Speed: 80 pixels/second
- Health: 2 HP

**How to Create New Enemy**:
```csharp
// 1. Create new file: scripts/Entity/EnemyCustom.cs
public class EnemyCustom : Enemy
{
    public EnemyCustom(Vector2 position, Art art, string name) 
        : base(position, art, name)
    {
        _maxHealth = 3;
        _health = 3;
        _attack = 2;
    }

    public override void Update()
    {
        // Handle entry animation first
        if (HandleEntry())
        {
            ClampToScreenBounds();
            return;
        }

        // Your custom AI logic here
        
        base.Update();
    }
}

// 2. Add to Enemy.MakeEnemy() in Enemy.cs
case "Custom":
    return new EnemyCustom(position, art, name);

// 3. Spawn in Scene.cs
AddEntity(new EnemyCustom(pos, Art.Enemy, "custom"));
```

---

#### **Projectile.cs**
**Purpose**: Ranged attack projectile for players and enemies

**Constructor**:
```csharp
Projectile(Vector2 position, Directions direction, Entity owner, 
           int damage, float speed = 300f)
```

**Properties**:
- `_lifeTime` - Despawn after 3 seconds
- `_damage` - Damage dealt on hit
- `_speed` - Movement speed
- `_velocity` - Direction * speed

**Usage Example**:
```csharp
// Player shoots
Projectile bullet = new Projectile(
    playerPos, 
    player.CurrentDirection, 
    player, 
    player.Attack, 
    350f
);
scene.AddEntity(bullet);

// Enemy shoots
Projectile enemyBullet = new Projectile(
    enemyPos,
    Directions.Down,
    enemy,
    1,
    200f
);
scene.AddEntity(enemyBullet);
```

---

#### **Coin.cs**
**Purpose**: Collectible coin with bobbing animation

**Properties**:
- `_value` - Coin worth (default: 1)
- `_bobSpeed` - Animation speed (2.0)
- `_bobHeight` - Bob amplitude (4 pixels)

**How It Works**:
- Automatically bobs up and down
- Collects on player touch
- Adds to GameState "Coins" counter

**Custom Collectible Example**:
```csharp
public class GemCollectible : SpriteEntity
{
    private int _points = 10;

    public GemCollectible(Vector2 position, Art art) : base(position, art)
    {
        _label = "collectible";
    }

    public override void Update()
    {
        base.Update();
        
        var hit = _scene.WhichEntityColliding(this, "player");
        if (hit is Player player)
        {
            int score = ServiceLocator.GameState.GetState<int>("Score");
            ServiceLocator.GameState.SetState("Score", score + _points);
            Die();
        }
    }
}
```

---

#### **Sword.cs**
**Purpose**: Melee weapon entity spawned in front of player

**Properties**:
- Short lifetime (controlled by timer)
- Collides with enemies
- Positioned based on player direction

**Usage**:
```csharp
// Spawned automatically in Player.HandleInput() when Z pressed
Sword sword = new Sword(swordPos, Art.Sword, player, player.Attack);
scene.AddEntity(sword);
```

---

### EntityType Files (scripts/EntityType/)

#### **Entity.cs** (Base Class)
**Purpose**: Base class for all game entities

**Key Properties**:
```csharp
protected Vector2 _position      // World position
protected Rectangle _rect        // Collision rectangle
protected Point _rect_offset     // Rectangle offset from position
protected string _label          // Entity label for collision filtering
protected Scene _scene           // Parent scene reference
```

**Key Methods**:
```csharp
void AddToScene(Scene scene)         // Called when added to scene
void RemoveFromScene(Scene scene)    // Called when removed
void Die()                           // Mark for removal
virtual void Update()                // Per-frame update
virtual void Draw(SpriteBatch batch) // Rendering
void Translate(Vector2 offset)       // Move by offset
```

---

#### **SpriteEntity.cs**
**Purpose**: Entity with sprite rendering

**Constructor**:
```csharp
SpriteEntity(Vector2 position, Art art)
```

**Properties**:
- `_currentArt` - Art enum for texture
- `_color` - Tint color
- Inherits all Entity properties

---

#### **AnimationEntity.cs**
**Purpose**: Entity with frame-by-frame animation

**Constructor**:
```csharp
AnimationEntity(Vector2 position, Texture2D[] frames, 
                float frameTime, bool loop = true)
```

**Usage Example**:
```csharp
// Death animation
AnimationEntity death = new AnimationEntity(
    position,
    AssetManager.DeathFrames,
    0.12f,  // 0.12 seconds per frame
    false   // Don't loop
);
scene.AddEntity(death);
```

---

#### **ButtonEntity.cs**
**Purpose**: UI button with click detection (template/stub)

**Usage**: Customize for your UI needs

---

#### **TextEntity.cs**
**Purpose**: Display text in game world (template/stub)

**Usage**: Customize for in-game text display

---

### GameScene Files (scripts/GameScene/)

#### **Grid.cs**
**Purpose**: Tile-based collision grid

**Key Methods**:
```csharp
void SetTile(Point gridPos, Tile tile)              // Place tile
Tile GetTile(Point gridPos)                         // Get tile
static Vector2 GetPixelPositionFromGridPosition(Point grid)
static Point GetGridPositionFromPixelPosition(Vector2 pixel)
bool IsSolid(Point gridPos)                         // Check if blocked
```

**Constants**:
- `TileSize` = 64 pixels

**Usage Example**:
```csharp
// Set tiles
grid.SetTile(new Point(5, 3), Tile.Wall);
grid.SetTile(new Point(5, 4), Tile.Floor);

// Check collision
if (grid.IsSolid(gridPosition)) {
    // Can't move here
}

// Convert coordinates
Vector2 worldPos = Grid.GetPixelPositionFromGridPosition(new Point(10, 10));
Point gridPos = Grid.GetGridPositionFromPixelPosition(new Vector2(640, 480));
```

---

#### **Tile.cs**
**Purpose**: Tile types and properties

**Tile Types**:
```csharp
Tile.Empty      // No collision
Tile.Wall       // Solid collision
Tile.Floor      // Walkable
Tile.Water      // Custom type
Tile.Lava       // Custom type
```

**Symbol Mapping**:
```csharp
tileSymbols["#"] = Tile.Wall
tileSymbols["."] = Tile.Floor
tileSymbols["~"] = Tile.Water
// Add your own in Tile.cs
```

---

#### **HUDDisplay.cs**
**Purpose**: On-screen UI display

**Current Display**:
- Health: X/X (red)
- Attack: X (white)
- Coins: X (gold)
- Sword: Yes/No (yellow)
- Controls hint (gray)

**How to Add Custom Display**:
```csharp
// In HUDDisplay.Draw() method
int customValue = ServiceLocator.GameState.GetState<int>("MyValue");
spriteBatch.DrawString(_font, $"MyValue: {customValue}", 
    new Vector2(20, 140), Color.Cyan);
```

---

#### **EnemySpawner.cs**
**Purpose**: Spawn waves of enemies (template)

**Usage**: Customize for wave-based spawning

---

### Screen Files (scripts/Screen/)

#### **GameScreen.cs** (Base Class)
**Purpose**: Base class for all screens

**Methods**:
```csharp
virtual void Load()                          // Initialize
virtual void Update(GameTime gameTime)       // Update
virtual void Draw(SpriteBatch spriteBatch)   // Draw
```

---

#### **SceneScreen.cs**
**Purpose**: Main gameplay screen containing a Scene

**Usage**: Primary game screen that holds the active scene

---

#### **StartScene.cs, WinScreen.cs, LoseScreen.cs**
**Purpose**: Menu/end screens (currently simplified/stubs)

**How to Restore**:
These were simplified during template conversion. Check git history to restore full implementations.

---

## How-To Guides

### How to Add a New Enemy Type

1. **Create Enemy Class**:
```csharp
// File: scripts/Entity/EnemyBoss.cs
using Microsoft.Xna.Framework;

namespace Template;

public class EnemyBoss : Enemy
{
    private float _speed = 50f;
    private float _shootTimer = 0f;

    public EnemyBoss(Vector2 position, Art art, string name) 
        : base(position, art, name)
    {
        _maxHealth = 10;
        _health = 10;
        _attack = 3;
    }

    public override void Update()
    {
        // Handle entry animation
        if (HandleEntry())
        {
            ClampToScreenBounds();
            return;
        }

        // Custom behavior
        Player player = _scene.GetPlayer();
        if (player != null)
        {
            // Chase player
            Vector2 toPlayer = player.Position - _position;
            if (toPlayer.Length() > 0)
            {
                Vector2 direction = Vector2.Normalize(toPlayer);
                Vector2 velocity = direction * _speed * ServiceLocator.DeltaSeconds;
                
                if (_scene != null)
                {
                    velocity = _scene.CheckForGridCollision(this, velocity);
                }
                Translate(velocity);
            }

            // Shoot periodically
            _shootTimer -= ServiceLocator.DeltaSeconds;
            if (_shootTimer <= 0)
            {
                // Shoot logic here
                _shootTimer = 2.0f;
            }
        }

        ClampToScreenBounds();
        base.Update();
    }
}
```

2. **Register in Enemy Factory**:
```csharp
// In Enemy.cs, MakeEnemy() method
case "Boss":
    return new EnemyBoss(position, art, name);
```

3. **Spawn in Scene**:
```csharp
// In Scene.SpawnEnemiesInScreen() or manually
AddEntity(new EnemyBoss(new Vector2(500, 400), Art.Enemy, "boss"));
```

---

### How to Add a New Entity Type

1. **Decide Base Class**:
   - Use `Entity` for non-visual entities
   - Use `SpriteEntity` for single-texture entities
   - Use `AnimationEntity` for animated entities

2. **Create Class**:
```csharp
// File: scripts/Entity/HealthPotion.cs
using Microsoft.Xna.Framework;

namespace Template;

public class HealthPotion : SpriteEntity
{
    private int _healAmount = 3;

    public HealthPotion(Vector2 position, Art art) : base(position, art)
    {
        _label = "potion";
        _rect_offset = new Point(8, 8);
        UpdateRectangle();
    }

    public override void Update()
    {
        base.Update();

        var hit = _scene.WhichEntityColliding(this, "player");
        if (hit is Player player)
        {
            player.Health = Math.Min(player.Health + _healAmount, player.MaxHealth);
            Die();
        }
    }

    private void UpdateRectangle()
    {
        _rect = new Rectangle(
            (int)_position.X + _rect_offset.X,
            (int)_position.Y + _rect_offset.Y,
            Grid.TileSize - _rect_offset.X * 2,
            Grid.TileSize - _rect_offset.Y * 2
        );
    }
}
```

3. **Spawn**:
```csharp
scene.AddEntity(new HealthPotion(position, Art.Potion));
```

---

### How to Load Grid from CSV

**Grid files are located in**: `Data/GridProject - [name].csv`

1. **Uncomment Load Methods** in `Scene.cs`:
```csharp
// Uncomment these methods (around line 98):
void LoadLevel(string levelName)
void ParseLevelText(string levelContent)
bool CheckForSpawnEntity(Point gridPosition, string symbol)
```

2. **Call LoadLevel in Constructor**:
```csharp
public Scene()
{
    // Uncomment this line in constructor
    LoadLevel("Zelda01");  // Load specific level
}
```

3. **CSV Format**:
```csv
#,#,#,#,#
#,.,.,.,#
#,.,player,.,#
#,.,enemy_spider,.,#
#,#,#,#,#
```

**Symbols**:
- `#` - Wall tile
- `.` - Floor tile
- `player` - Player spawn
- `enemy_[type]` - Enemy spawn
- `door_[destination]` - Door to other scene

4. **Add Custom Symbols** in `CheckForSpawnEntity()`:
```csharp
case "chest":
    AddEntity(new Chest(pixelPosition, Art.Chest));
    return true;
```

---

### How to Change Scenes

**Method 1: Direct Change** (Current system)
```csharp
// In any scene code
ChangeScene("Zelda02");
```

**Method 2: Edge Transitions** (Requires uncommenting)

1. **Uncomment EdgeTransition methods** in `Scene.cs` (line ~148)

2. **Check Edge in Player.cs** (currently commented around line 250):
```csharp
// Uncomment edge checking code in Player.Update()
if (_position.X < Scene.SceneOffset.X) {
    ServiceLocator.GameState.QueueEdgeTransition(
        GameState.SpawnEdge.Right, _position);
    _scene.ChangeScene("LeftScene");
}
```

3. **Call EdgeTransition** in Scene constructor:
```csharp
public Scene()
{
    EdgeTransition(); // Uncomment this
    LoadLevel("Zelda01");
}
```

**Method 3: Door Transitions**

1. **Add Door in CSV**:
```csv
door_Zelda02
```

2. **Door automatically spawned** by CheckForSpawnEntity

3. **Player collides with door** - scene changes

---

### How to Create Custom Screen

```csharp
// File: scripts/Screen/PauseScreen.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public class PauseScreen : GameScreen
{
    public override void Load()
    {
        // Initialize resources
    }

    public override void Update(GameTime gameTime)
    {
        // Check for unpause input
        if (ServiceLocator.Input.IsActionPressed(Action.Start))
        {
            // Return to game
            ScreenManager.ChangeScreen(new SceneScreen());
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        spriteBatch.DrawString(AssetManager.font, "PAUSED", 
            new Vector2(400, 300), Color.White);
        spriteBatch.End();
    }
}

// Switch to pause screen
ScreenManager.ChangeScreen(new PauseScreen());
```

---

### How to Save/Load Custom Data

```csharp
// Save custom game state
ServiceLocator.GameState.SetState("PlayerLevel", 5);
ServiceLocator.GameState.SetState("QuestCompleted", true);
ServiceLocator.GameState.SetState("InventoryCount", 25);
ServiceLocator.GameState.SetState("LastCheckpoint", "Village");

// Save to file (1 key or manual)
ServiceLocator.GameState.SaveToFile("save_slot1.json");

// Load from file (2 key or manual)
ServiceLocator.GameState.LoadFromFile("save_slot1.json");

// Retrieve after loading
int level = ServiceLocator.GameState.GetState<int>("PlayerLevel");
bool questDone = ServiceLocator.GameState.GetState<bool>("QuestCompleted");
string checkpoint = ServiceLocator.GameState.GetState<string>("LastCheckpoint");

// Load with defaults
int coins = ServiceLocator.GameState.GetState<int>("Coins", 0); // Default 0
```

**JSON Format** (`save.json`):
```json
{
  "Timestamp": "2025-12-07T10:30:00",
  "StateData": {
    "Coins": 25,
    "PlayerLevel": 5,
    "QuestCompleted": true,
    "LastCheckpoint": "Village"
  },
  "PlayerHealth": 6,
  "PlayerMaxHealth": 6,
  "HasSword": true,
  "CurrentSceneName": "Zelda01"
}
```

---

### How to Add Mouse Interactions

**Click Detection**:
```csharp
if (Input.IsLeftMouseClick())
{
    Vector2 clickPos = Input.GetMousePosition();
    // Handle click at position
}
```

**Drag Detection**:
```csharp
if (Input.IsMouseDragging())
{
    Vector2 dragDelta = Input.GetMouseDragDelta();
    // Move something by dragDelta
}
```

**Hover Detection**:
```csharp
if (Input.IsMouseHovering())
{
    Vector2 mousePos = Input.GetMousePosition();
    // Check if mouse over UI element
    Rectangle buttonRect = new Rectangle(100, 100, 200, 50);
    if (buttonRect.Contains(mousePos))
    {
        // Show hover effect
    }
}
```

**Custom Button Example**:
```csharp
public class Button
{
    private Rectangle _bounds;
    private bool _isHovered;

    public void Update()
    {
        Vector2 mousePos = Input.GetMousePosition();
        _isHovered = _bounds.Contains(mousePos);

        if (_isHovered && Input.IsLeftMouseClick())
        {
            OnClick();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = _isHovered ? Color.Yellow : Color.White;
        // Draw button with color
    }

    private void OnClick()
    {
        // Button action
    }
}
```

---

## API Reference

### ServiceLocator (Manager/Services.cs)
**Purpose**: Global service access

```csharp
ServiceLocator.GameState    // GameState instance
ServiceLocator.Input        // Input instance
ServiceLocator.DeltaSeconds // Delta time (float)
ServiceLocator.GameTime     // GameTime instance
```

---

### AssetManager (Entity/AssetManager.cs)
**Purpose**: Asset loading and storage

**Enums**:
```csharp
enum Art { Player, Enemy, Sword, Coin, Cave, etc. }
enum Item { None, Sword, Coin }
enum Directions { Up, Down, Left, Right }
```

**Properties**:
```csharp
static SpriteFont font
static Texture2D[] DeathFrames
static Dictionary<Art, Texture2D> textures
```

**Methods**:
```csharp
static void LoadAssets(ContentManager content, GraphicsDevice device)
static Texture2D GetTexture(Art art)
```

---

### Grid Constants

```csharp
Grid.TileSize = 64;  // Pixels per tile
Scene.SceneOffset = new Vector2(0, 224);  // HUD height
Scene.GameSceneSize = new Vector2(1024, 704);  // Playable area
Game1.ScreenSize = new Vector2(1024, 928);  // Total screen
```

---

### Common Patterns

**Entity Collision**:
```csharp
var hit = _scene.WhichEntityColliding(this, "enemy");
if (hit is Enemy enemy)
{
    enemy.TakeDamage(5);
}
```

**Grid Collision**:
```csharp
Vector2 velocity = new Vector2(100, 0) * deltaTime;
velocity = _scene.CheckForGridCollision(this, velocity);
if (velocity.X != 0 || velocity.Y != 0)
{
    Translate(velocity);
}
```

**Spawning Entities**:
```csharp
// In scene or entity Update/logic
Entity newEntity = new MyEntity(position, art);
_scene.AddEntity(newEntity);
```

**Death/Removal**:
```csharp
// Mark for removal (cleaned up automatically)
Die();
```

---

## Advanced Topics

### Multi-Scene Architecture

**Current Setup**: Single-screen arena mode with random spawning

**To Enable Multi-Scene**:
1. Uncomment CSV loading in `Scene.cs`
2. Create CSV files in `Data/` folder
3. Uncomment edge transition code
4. Set scene names in ChangeScene() calls

**Scene Naming Convention**:
- `Zelda01`, `Zelda02`, etc. - Overworld scenes
- `Underworld` - Dungeon scene
- Custom names work too

---

### Performance Tips

1. **Entity Pooling**: Reuse projectiles instead of creating new ones
2. **Spatial Partitioning**: For many entities, use grid-based lookup
3. **Update Culling**: Don't update off-screen entities
4. **Draw Batching**: Batch similar sprites in SpriteBatch

---

### Debugging

**Enable Debug Info**:
```csharp
// In Entity.Draw() or Scene.Draw()
if (Game1.Debug)
{
    // Draw collision rectangles
    spriteBatch.DrawRectangle(_rect, Color.Red);
    
    // Draw labels
    spriteBatch.DrawString(AssetManager.font, _label, 
        _position, Color.Yellow);
}
```

**Console Logging**:
```csharp
Console.WriteLine($"Player Health: {player.Health}");
Debug.WriteLine($"Entities: {_entities.Count}");
```

---

## Common Issues & Solutions

### Issue: Entities not colliding
**Solution**: Check `_label` matches WhichEntityColliding() parameter

### Issue: Save/Load not working
**Solution**: Ensure ServiceLocator.GameState is initialized in Game1.cs

### Issue: Mouse clicks not detected
**Solution**: Call `Input.Update()` in Game1.Update()

### Issue: Enemies spawn in walls
**Solution**: Use scene.CheckForGridCollision() or adjust spawn positions

### Issue: Scene doesn't change
**Solution**: Check that ChangeScene() is called and scene name is correct

---

## Extending the Template

### Add New Game Modes

**Example: Survival Mode**
```csharp
public class SurvivalMode
{
    private float _waveTimer = 30f;
    private int _currentWave = 1;

    public void Update(Scene scene)
    {
        _waveTimer -= ServiceLocator.DeltaSeconds;
        if (_waveTimer <= 0)
        {
            SpawnWave(scene, _currentWave);
            _currentWave++;
            _waveTimer = 30f;
        }
    }

    private void SpawnWave(Scene scene, int wave)
    {
        int enemyCount = 3 + wave * 2;
        for (int i = 0; i < enemyCount; i++)
        {
            // Spawn enemies
        }
    }
}
```

### Add Quest System

```csharp
public class Quest
{
    public string Name;
    public string Description;
    public bool IsComplete;
    public System.Action OnComplete;

    public void CheckCompletion()
    {
        // Check quest conditions
        if (/* condition met */)
        {
            IsComplete = true;
            OnComplete?.Invoke();
        }
    }
}

// Usage
Quest killQuest = new Quest {
    Name = "Defeat Enemies",
    Description = "Defeat 10 enemies",
    OnComplete = () => {
        ServiceLocator.GameState.SetState("Coins", 
            ServiceLocator.GameState.GetState<int>("Coins") + 50);
    }
};
```

---

## Credits & License

This template is designed for educational purposes. Customize and extend as needed for your projects.

**Built with**:
- MonoGame 3.8.4.1
- .NET 8.0
- C#

**Key Features**:
- ✅ Entity-Component System
- ✅ Scene Management
- ✅ JSON Save/Load
- ✅ Mouse & Keyboard Input
- ✅ Grid-based Collision
- ✅ Sprite & Animation Support
- ✅ Enemy AI (Spider, Shooter, Chaser)
- ✅ Projectile Combat
- ✅ Collectibles System

---

For more help, check the inline code comments or refer to specific file documentation above.
