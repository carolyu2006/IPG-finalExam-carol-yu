using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;

namespace Template;

public enum Directions
{
    Up,
    Down,
    Left,
    Right,
}

public enum PlayerState
{
    Walking,
    Attacking,
    UsingItem,
    Damaged
}

public class Player : SpriteEntity

{
    #region Fields and Properties
    private int _maxHealth = 6;
    private int _health;
    private int _attack = 1;
    private float _invulTimer = 0f;
    private const float InvulDuration = 1f; // seconds of invulnerability after hit

    public int Health => _health;
    public int MaxHealth => _maxHealth;
    public int Attack => _attack;

    public Directions CurrentDirection { get; private set; } = Directions.Up;
    private Directions _lastDirection = Directions.Down;

    private Directions? _lastPressedDirection = null;

    public PlayerState CurrentState { get; set; } = PlayerState.Walking;

    protected float speed = 5f;

    Vector2 desiredDirection = Vector2.Zero;

    // Mouse movement
    private Vector2? _mouseTargetPosition = null;
    private const float MouseMoveThreshold = 5f; // Distance threshold to stop moving

    // Attack cooldowns
    private float _attackTimer = 0f;
    private const float AttackDuration = 0.18f;

    // Shooting cooldown
    private float _shootTimer = 0f;
    private const float ShootCooldown = 0.5f; // Half second between shots

    private int _animFrame = 0;
    private float _animDistance = 0f;
    private const float AnimStep = 35f;

    private float _damageFrameTimer = 0f;
    private int _damageFrameIndex = 0;
    // faster damaged-frame playback
    private const float DamageFrameTime = 0.06f;
    #endregion

    #region Constructor

    public Player(Vector2 position, Art art) : base(position, art)
    {
        _label = "player";
        _rect_offset = new Point(5, Grid.TileSize / 2);
        _rect = new Rectangle(
            (int)_position.X + _rect_offset.X,
            (int)_position.Y + _rect_offset.Y,
            Grid.TileSize - _rect_offset.X * 2,
            Grid.TileSize - _rect_offset.Y
        );
        UpdateTextureFrame(reset: true);

        if (ServiceLocator.GameState != null)
        {
            // adopt global max if set
            _maxHealth = Math.Max(2, ServiceLocator.GameState.PlayerMaxHealth);
            _health = Math.Clamp(ServiceLocator.GameState.PlayerHealth, 0, _maxHealth);
            ServiceLocator.GameState.Player = this;
        }
        else
        {
            _health = _maxHealth;
        }

        if (ServiceLocator.GameState != null)
        {
            ServiceLocator.GameState.Player = this;
        }
    }

    public void EquipSword()
    {
        _attack = Math.Max(_attack, 2);
    }
    #endregion

    #region Update
    public override void Update()
    {
        HandleInput();
        Vector2 velocity = desiredDirection * speed;
        Vector2 startPos = _position;

        if (_scene != null)
        {
            Vector2 velocityX = new Vector2(velocity.X, 0);
            velocityX = _scene.CheckForGridCollision(this, velocityX);
            if (velocityX.X != 0)
            {
                Translate(new Vector2(velocityX.X, 0));
            }

            Vector2 velocityY = new Vector2(0, velocity.Y);
            velocityY = _scene.CheckForGridCollision(this, velocityY);
            if (velocityY.Y != 0)
            {
                Translate(new Vector2(0, velocityY.Y));
            }
        }
        else
        {
            Translate(velocity);
        }

        #region Animation and State Updates
        //animate walk
        float desiredDist = Math.Abs(velocity.X) + Math.Abs(velocity.Y);
        if (desiredDist > 0.01f)
        {
            CurrentState = PlayerState.Walking;
            _animDistance += desiredDist;
            if (_animDistance >= AnimStep)
            {
                _animDistance -= AnimStep;
                _animFrame = (_animFrame + 1) % 2; // two walking frames
            }
        }
        if (CurrentDirection != _lastDirection)
        {
            _animFrame = 0;
            _animDistance = 0f;
            _lastDirection = CurrentDirection;
        }

        // invulnerability timer update
        if (_invulTimer > 0f)
        {
            CurrentState = PlayerState.Damaged;
            _invulTimer -= ServiceLocator.DeltaSeconds;
            if (_invulTimer < 0f)
                _invulTimer = 0f;
        }

        // attack timer update
        if (_attackTimer > 0f)
        {
            _attackTimer -= ServiceLocator.DeltaSeconds;
            if (_attackTimer <= 0f)
            {
                _attackTimer = 0f;
                if (CurrentState == PlayerState.Attacking)
                {
                    CurrentState = PlayerState.Walking;
                }
            }
        }

        // shoot timer update
        if (_shootTimer > 0f)
        {
            _shootTimer -= ServiceLocator.DeltaSeconds;
            if (_shootTimer < 0f) _shootTimer = 0f;
        }

        if (CurrentState == PlayerState.Damaged)
        {
            if (AssetManager.PlayerAnimations.TryGetValue((CurrentDirection, PlayerState.Damaged), out var dmgFrames) && dmgFrames.Length > 0)
            {
                _damageFrameTimer += ServiceLocator.DeltaSeconds;
                if (_damageFrameTimer >= DamageFrameTime)
                {
                    _damageFrameTimer -= DamageFrameTime;
                    int prev = _damageFrameIndex;
                    _damageFrameIndex = (_damageFrameIndex + 1) % dmgFrames.Length;
                    // Console.WriteLine($"[Player] Damage frame advanced {prev} -> {_damageFrameIndex} (len={dmgFrames.Length})");
                }
            }
            else
            {
                _damageFrameTimer = 0f;
                _damageFrameIndex = 0;
            }
        }
        else
        {
            _damageFrameTimer = 0f;
            _damageFrameIndex = 0;
        }
        #endregion

        UpdateTextureFrame();

        #region door
        // Door/Scene transitions disabled - uncomment to re-enable
        Door door = Scene.WhichEntityColliding(this, "door") as Door;
        if (door != null)
        {
            if (Scene is Scene currentScene)
            {
                ServiceLocator.GameState.QueueEdgeTransition(
                    GameState.SpawnEdge.FromDoor,
                    Position
                );
            }
            _scene.ChangeScene(door.NextScene);
        }
        #endregion

        #region Item Pickup
        ItemPickUp pickup = _scene.WhichEntityColliding(this, "item") as ItemPickUp;
        if (pickup != null && !pickup.isDead)
        {
            if (Scene is Scene currentScene)
            {
                var gridPos = Grid.GetGridPositionFromPixelPosition(pickup.Position);
                ServiceLocator.GameState.MarkItemCollected(currentScene.LevelName, gridPos);
            }
            ServiceLocator.GameState.PickUpItem(pickup);
        }
        #endregion

        base.Update();
    }
    #endregion

    #region Input Handling
    void HandleInput()
    {
        if (ServiceLocator.GameState != null && !ServiceLocator.GameState.AllowPlayerControl)
        {
            desiredDirection = Vector2.Zero;
            _mouseTargetPosition = null;
            return;
        }
        // Edge transitions disabled - uncomment CheckEdgeTransition() to re-enable scene switching
        // CheckEdgeTransition();

        // Check for mouse click to set target position
        if (Input.IsLeftMouseClick())
        {
            _mouseTargetPosition = Input.GetMousePosition();
        }

        // Handle mouse movement to target
        bool hasKeyboardInput = false;

        // Track key-down events (pressed this frame) to update recency
        bool leftPressed = ServiceLocator.Input.IsActionPressed(Action.MoveLeft);
        bool rightPressed = ServiceLocator.Input.IsActionPressed(Action.MoveRight);
        bool upPressed = ServiceLocator.Input.IsActionPressed(Action.MoveUp);
        bool downPressed = ServiceLocator.Input.IsActionPressed(Action.MoveDown);

        if (leftPressed) _lastPressedDirection = Directions.Left;
        if (rightPressed) _lastPressedDirection = Directions.Right;
        if (upPressed) _lastPressedDirection = Directions.Up;
        if (downPressed) _lastPressedDirection = Directions.Down;

        // Current held states
        bool leftDown = ServiceLocator.Input.IsActionDown(Action.MoveLeft);
        bool rightDown = ServiceLocator.Input.IsActionDown(Action.MoveRight);
        bool upDown = ServiceLocator.Input.IsActionDown(Action.MoveUp);
        bool downDown = ServiceLocator.Input.IsActionDown(Action.MoveDown);

        hasKeyboardInput = leftDown || rightDown || upDown || downDown;

        desiredDirection = Vector2.Zero;
        Directions? chosenDirection = null;

        #region move
        // Keyboard input takes priority over mouse movement
        if (hasKeyboardInput)
        {
            // Cancel mouse movement if keyboard is used
            _mouseTargetPosition = null;

            if (_lastPressedDirection.HasValue)
            {
                switch (_lastPressedDirection.Value)
                {
                    case Directions.Left: if (leftDown) chosenDirection = Directions.Left; break;
                    case Directions.Right: if (rightDown) chosenDirection = Directions.Right; break;
                    case Directions.Up: if (upDown) chosenDirection = Directions.Up; break;
                    case Directions.Down: if (downDown) chosenDirection = Directions.Down; break;
                }
            }

            // If last pressed isn't held anymore, pick any currently held key
            if (!chosenDirection.HasValue)
            {
                if (leftDown) chosenDirection = Directions.Left;
                else if (rightDown) chosenDirection = Directions.Right;
                else if (upDown) chosenDirection = Directions.Up;
                else if (downDown) chosenDirection = Directions.Down;
            }

            // Apply the chosen direction
            if (chosenDirection.HasValue)
            {
                switch (chosenDirection.Value)
                {
                    case Directions.Left: desiredDirection = new Vector2(-1, 0); break;
                    case Directions.Right: desiredDirection = new Vector2(1, 0); break;
                    case Directions.Up: desiredDirection = new Vector2(0, -1); break;
                    case Directions.Down: desiredDirection = new Vector2(0, 1); break;
                }
                CurrentDirection = chosenDirection.Value;
            }
        }
        else if (_mouseTargetPosition.HasValue)
        {
            // Move towards mouse target
            Vector2 playerCenter = _position + new Vector2(_rect.Width / 2, _rect.Height / 2);
            Vector2 directionToTarget = _mouseTargetPosition.Value - playerCenter;
            float distanceToTarget = directionToTarget.Length();

            if (distanceToTarget > MouseMoveThreshold)
            {
                // Normalize and set as desired direction
                desiredDirection = Vector2.Normalize(directionToTarget);

                // Update facing direction based on movement
                if (Math.Abs(directionToTarget.X) > Math.Abs(directionToTarget.Y))
                {
                    CurrentDirection = directionToTarget.X > 0 ? Directions.Right : Directions.Left;
                }
                else
                {
                    CurrentDirection = directionToTarget.Y > 0 ? Directions.Down : Directions.Up;
                }
            }
            else
            {
                // Reached target
                _mouseTargetPosition = null;
            }
        }
        #endregion

        #region attack
        //attack
        if (ServiceLocator.Input.IsActionPressed(Action.ActionA) && ServiceLocator.GameState != null && ServiceLocator.GameState.HasSword)
        {
            // Only allow if not currently attacking
            if (_attackTimer <= 0f && _scene != null)
            {
                Vector2 swordPos = Position + new Vector2(Grid.TileSize / 2, Grid.TileSize / 2);
                // choose the direction-specific sword texture (fallback to the generic sword art)
                Texture2D swordTexture = AssetManager.GetTexture(Art.sword);
                switch (CurrentDirection)
                {
                    case Directions.Left:
                        swordPos += new Vector2(-Grid.TileSize, 0);
                        swordTexture = AssetManager.SwordDirection[Directions.Left];
                        break;
                    case Directions.Right:
                        swordPos += new Vector2(0, -10);
                        swordTexture = AssetManager.SwordDirection[Directions.Right];
                        break;
                    case Directions.Up:
                        swordPos += new Vector2(-36, -Grid.TileSize - 22);
                        swordTexture = AssetManager.SwordDirection[Directions.Up];
                        break;
                    case Directions.Down:
                        swordPos += new Vector2(-10, Grid.TileSize);
                        swordTexture = AssetManager.SwordDirection[Directions.Down];
                        break;
                }

                // Create the sword with the chosen texture
                Sword swing = new Sword(swordPos, swordTexture, this, CurrentDirection);
                _scene.AddEntity(swing);
                _attackTimer = AttackDuration;
                CurrentState = PlayerState.Attacking;
            }
        }

        // Ranged attack - Right click or ActionB to shoot projectile
        if ((Input.IsRightMouseClick() || ServiceLocator.Input.IsActionPressed(Action.ActionB))
            && _shootTimer <= 0f && _scene != null)
        {
            // Calculate projectile spawn position (in front of player)
            Vector2 projectilePos = Position + new Vector2(Grid.TileSize / 2, Grid.TileSize / 2);

            switch (CurrentDirection)
            {
                case Directions.Left:
                    projectilePos += new Vector2(-Grid.TileSize / 2, 0);
                    break;
                case Directions.Right:
                    projectilePos += new Vector2(Grid.TileSize / 2, 0);
                    break;
                case Directions.Up:
                    projectilePos += new Vector2(0, -Grid.TileSize / 2);
                    break;
                case Directions.Down:
                    projectilePos += new Vector2(0, Grid.TileSize / 2);
                    break;
            }

            // Create and spawn projectile
            Projectile projectile = new Projectile(projectilePos, CurrentDirection, this, _attack, 350f);
            _scene.AddEntity(projectile);
            _shootTimer = ShootCooldown;
            CurrentState = PlayerState.Attacking;
        }
        #endregion
    }
    #endregion

    #region Update Texture
    void UpdateTextureFrame(bool reset = false)
    {
        if (reset)
        {
            _animFrame = 0;
            _animDistance = 0f;
        }

        Texture2D[] frames;
        if (CurrentState == PlayerState.Walking)
        {
            if (AssetManager.PlayerAnimations.TryGetValue((CurrentDirection, PlayerState.Walking), out frames))
            {
                if (frames.Length > 0)
                {
                    int idx = Math.Clamp(_animFrame, 0, frames.Length - 1);
                    _texture = frames[idx];
                }
            }
        }
        else if (CurrentState == PlayerState.Attacking)
        {
            if (AssetManager.PlayerAnimations.TryGetValue((CurrentDirection, PlayerState.Attacking), out frames))
            {
                if (frames.Length > 0)
                {
                    float elapsed = AttackDuration - _attackTimer;
                    if (elapsed < 0f) elapsed = 0f;
                    float progress = AttackDuration > 0f ? (elapsed / AttackDuration) : 1f;
                    int idx = (int)Math.Floor(progress * frames.Length);
                    if (idx >= frames.Length) idx = frames.Length - 1;
                    if (idx < 0) idx = 0;
                    _texture = frames[idx];
                }
            }
        }
        else if (CurrentState == PlayerState.Damaged)
        {
            if (AssetManager.PlayerAnimations.TryGetValue((CurrentDirection, PlayerState.Damaged), out frames))
            {
                if (frames.Length > 0)
                {
                    int idx = Math.Clamp(_damageFrameIndex, 0, frames.Length - 1);
                    // Console.WriteLine($"[Player] Select damaged frame idx={idx} (len={frames.Length})");
                    _texture = frames[idx];
                }
            }
        }
    }
    #endregion

    #region Damage Handling

    public void TakeDamage(int amount)
    {
        Console.WriteLine("health" + _health);
        if (_invulTimer > 0f) return; // currently invulnerable

        int dmg = Math.Max(1, amount);
        _health -= dmg;
        _invulTimer = InvulDuration;
        CurrentState = PlayerState.Damaged;

        if (_health <= 0)
        {
            _health = 0;
            Die();
            AnimationEntity dieAnim = new AnimationEntity(_position, AssetManager.DeathFrames, 0.12f, false)
            {
                OnFinished = async () =>
                {
                    await Task.Delay(500);
                    ScreenManager.ChangeScreen(new LoseScreen());
                }
            };
            _scene?.AddEntity(dieAnim);
        }
        if (ServiceLocator.GameState != null)
        {
            // Console.WriteLine(desiredDirection);
            int desiredX = (int)desiredDirection.X * 100;
            int desiredY = (int)desiredDirection.Y * 100;
            _position = new Vector2((int)Position.X - desiredX, (int)Position.Y - desiredY);
            ServiceLocator.GameState.PlayerHealth = _health;
            ServiceLocator.GameState.PlayerMaxHealth = _maxHealth;
        }
    }

    #endregion

    #region Edge Transition (Commented out - for multi-scene navigation)
    /*
    void CheckEdgeTransition()
    {
        if (_scene == null) return;

        // Scene offset (HUD height)
        float sceneTop = Scene.SceneOffset.Y;
        float sceneBottom = sceneTop + Scene.GameSceneSize.Y;
        float sceneLeft = 0;
        float sceneRight = Scene.GameSceneSize.X;

        string nextMap = null;
        GameState.SpawnEdge? spawnEdge = null;

        // Check left edge
        if (_scene.LevelName != "Underworld")
        {
            if (Position.X < sceneLeft - _rect.Width)
            {
                nextMap = GetAdjacentMap(_scene.LevelName, -1, 0);
                spawnEdge = GameState.SpawnEdge.Right;
            }
            // Check right edge
            else if (Position.X > sceneRight)
            {
                nextMap = GetAdjacentMap(_scene.LevelName, 1, 0);
                spawnEdge = GameState.SpawnEdge.Left;
            }
            // Check top edge
            else if (Position.Y < sceneTop - _rect.Height)
            {
                nextMap = GetAdjacentMap(_scene.LevelName, 0, -1);
                spawnEdge = GameState.SpawnEdge.Bottom;
            }
            // Check bottom edge
            else if (Position.Y > sceneBottom)
            {
                nextMap = GetAdjacentMap(_scene.LevelName, 0, 1);
                spawnEdge = GameState.SpawnEdge.Top;
            }
        }
        else
        {
            if (Position.Y > sceneBottom)
            {
                nextMap = "Zelda12";
                spawnEdge = GameState.SpawnEdge.Door;
            }
        }

        if (nextMap != null && spawnEdge.HasValue)
        {
            ServiceLocator.GameState.QueueEdgeTransition(spawnEdge.Value, Position);
            _scene.ChangeScene(nextMap);
        }
    }

    string GetAdjacentMap(string currentMap, int deltaX, int deltaY)
    {
        if (currentMap.Length < 7 || !currentMap.StartsWith("Zelda")) return null;

        if (!int.TryParse(currentMap.Substring(5, 1), out int row)) return null;
        if (!int.TryParse(currentMap.Substring(6, 1), out int col)) return null;

        int newRow = row + deltaY;
        int newCol = col + deltaX;

        if (newRow < 0 || newRow > 1 || newCol < 1 || newCol > 3) return null;
        Console.WriteLine($"Adjacent map: Zelda{newRow}{newCol}");
        return $"Zelda{newRow}{newCol}";
    }
    */
    #endregion

    #region draw
    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = _texture;
        if (tex == null)
        {
            base.Draw(spriteBatch);
            return;
        }

        float drawX = _position.X;
        float drawY = _position.Y;

        int baseSize = Grid.TileSize;

        if (CurrentState == PlayerState.Attacking)
        {
            switch (CurrentDirection)
            {
                case Directions.Left:
                    drawX = _position.X - (tex.Width - baseSize);
                    drawY = _position.Y - Math.Max(0, tex.Height - baseSize);
                    break;
                case Directions.Right:
                    drawX = _position.X;
                    drawY = _position.Y - Math.Max(0, tex.Height - baseSize);
                    break;
                case Directions.Up:

                    drawY = _position.Y - (tex.Height - baseSize);
                    break;
                case Directions.Down:
                    drawY = _position.Y;
                    break;
            }
        }

        Vector2 finalPos = new Vector2(drawX, drawY);
        if (_label != "ui") finalPos += ScreenManager.TransitionOffset;

        spriteBatch.Draw(tex, finalPos, Color.White);
    }
    #endregion
}