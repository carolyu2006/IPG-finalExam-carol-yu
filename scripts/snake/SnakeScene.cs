using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Template;

/// <summary>
/// Lightweight snake game screen with three food types (apple/banana/blueberry).
/// </summary>
public class SnakeScene : GameScreen
{
    #region data
    public bool isGameJustStarted = true;
    public bool addSegment = false;
    public static levelEnum level = levelEnum.normal;

    private const int TileSize = 32;
    private const int Columns = 24;   // 24 * 32 = 768px wide
    private const int Rows = 24;      // 18 * 32 = 576px tall
    private readonly Vector2 _boardOrigin = new Vector2(64, 64); // small margin

    private readonly List<Point> _snake = new();
    private Directions _direction = Directions.Right;
    private Directions _nextDirection = Directions.Right;

    private float _moveTimer = 0f;
    private float _moveInterval = 1f; // seconds between steps

    private readonly Random _rand = new();
    private FoodItem _food;
    private SpikeItem[] _spikeslist;
    private SpikeItem _spike;
    private int _score = 0;
    private bool _gameOver = false;

    private int foodCount = 0;
    private int totalFoodCount = 0;

    private Texture2D _pixel;

    private int spawnSpikeCount1 = 12;
    private int spawnSpikeCount2 = 20;

    private bool spikeSpawned = false;
    private bool spike2Spawned = false;

    #endregion

    public override void Load()
    {
        _pixel = AssetManager.GetTexture(Art.Pixel);
        ResetGame();
    }

    #region update
    public override void Update(GameTime gameTime)
    {
        HandleInput();

        if (_gameOver)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                ResetGame();
            }
            return;
        }



        if (_snake.Count >= spawnSpikeCount1 && !spikeSpawned)
        {
            SpawnSpike();
            spikeSpawned = true;
        }
        if (_snake.Count >= spawnSpikeCount2 && !spike2Spawned)
        {
            SpawnSpikes();
            spike2Spawned = true;
        }

        _moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_moveTimer >= _moveInterval)
        {
            _moveTimer -= _moveInterval;
            StepSnake();
        }
    }
    #endregion

    #region draw
    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.Black);

        Rectangle boardRect = new Rectangle((int)_boardOrigin.X, (int)_boardOrigin.Y, Columns * TileSize, Rows * TileSize);
        spriteBatch.Draw(_pixel, boardRect, new Color(20, 20, 20));

        // food
        Color foodColor = Color.White;
        switch (_food.Type)
        {
            case FoodType.Apple:
                foodColor = Color.Red;
                break;
            case FoodType.Banana:
                foodColor = Color.Yellow;
                break;
            case FoodType.Blueberry:
                foodColor = Color.Blue;
                break;
            default:
                foodColor = Color.White;
                break;
        }

        spriteBatch.Draw(_pixel, ToRect(_food.Position), foodColor);

        // spike
        if (_snake.Count >= spawnSpikeCount1 && _spikeslist != null)
        {
            foreach (var spike in _spikeslist)
                spriteBatch.Draw(_pixel, ToRect(spike.Position), Color.White);
        }
        // snake
        for (int i = _snake.Count - 1; i >= 0; i--)
        {
            Point p = _snake[i];
            Color c = i == 0 ? new Color(250, 250, 250) : new Color(180, 180, 180);
            spriteBatch.Draw(_pixel, ToRect(p), c);
        }

        spriteBatch.DrawString(AssetManager.font, $"Score: {_score}", new Vector2(100, 100), Color.White);
        spriteBatch.DrawString(AssetManager.font, $"Level: {level}", new Vector2(100, 130), Color.White);
        spriteBatch.DrawString(AssetManager.font, $"FoodCount: {foodCount}", new Vector2(100, 160), Color.White);
        spriteBatch.DrawString(AssetManager.font, $"SnakeCount: {_snake.Count}", new Vector2(100, 190), Color.White);
        spriteBatch.DrawString(AssetManager.font, $"Speed: {_moveInterval}", new Vector2(100, 220), Color.White);
        if (_gameOver)
        {
            string overMessage = "Game Over - Press Enter to restart";
            Vector2 size = AssetManager.font.MeasureString(overMessage);
            Vector2 pos = new Vector2(boardRect.Center.X - size.X / 2, boardRect.Center.Y - size.Y / 2);
            spriteBatch.DrawString(AssetManager.font, overMessage, pos, Color.White);
        }
    }
    #endregion

    #region handle input
    private void HandleInput()
    {
        KeyboardState ks = Keyboard.GetState();

        if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up))
        {
            if (_direction != Directions.Down) _nextDirection = Directions.Up;
        }
        else if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down))
        {
            if (_direction != Directions.Up) _nextDirection = Directions.Down;
        }
        else if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))
        {
            if (_direction != Directions.Right) _nextDirection = Directions.Left;
        }
        else if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))
        {
            if (_direction != Directions.Left) _nextDirection = Directions.Right;
        }
    }
    #endregion

    #region snake
    private void StepSnake()
    {
        _direction = _nextDirection;
        Point head = _snake[0];
        Point delta = new();
        switch (_direction)
        {
            case Directions.Up:
                delta = new Point(0, -1);
                break;
            case Directions.Down:
                delta = new Point(0, 1);
                break;
            case Directions.Left:
                delta = new Point(-1, 0);
                break;
            case Directions.Right:
                delta = new Point(1, 0);
                break;
        }

        Point newHead = head + delta;

        // wall
        if (newHead.X < 0 || newHead.X >= Columns || newHead.Y < 0 || newHead.Y >= Rows)
        {
            _gameOver = true;
            return;
        }

        // self
        for (int i = 0; i < _snake.Count; i++)
        {
            if (_snake[i] == newHead)
            {
                _gameOver = true;
                return;
            }
        }

        _snake.Insert(0, newHead);

        if (newHead == _food.Position)
        {
            int segmentsToAdd = foodSegmentsDict[_food.Type];

            for (int i = 0; i < segmentsToAdd; i++)
            {
                _snake.Add(_snake[^1]);
                Console.WriteLine("Adding segment");
            }

            foodCount += 1;
            totalFoodCount += 1;
            // Console.WriteLine("food count: " + totalFoodCount);
            _score += _food.Points;
            if (foodCount > 2)
            {
                _moveInterval *= 0.7f;
                // Console.WriteLine("Increasing speed!" + _moveInterval);
                foodCount = 0;
            }
            SpawnFood(); // spawn food
        }
        else if (_snake.Count >= spawnSpikeCount1 && _spikeslist != null)
        {
            foreach (var spike in _spikeslist)
            {
                if (newHead == spike.Position)
                {
                    _gameOver = true;
                    return;
                }
            }
            _snake.RemoveAt(_snake.Count - 1);
        }
        else
        {
            _snake.RemoveAt(_snake.Count - 1);
        }
    }
    #endregion


    #region reset game
    private void ResetGame()
    {
        isGameJustStarted = true;

        _snake.Clear();
        Point start = new Point(Columns / 2, Rows / 2);
        _snake.Add(start);
        _snake.Add(start + new Point(-1, 0));
        _snake.Add(start + new Point(-2, 0));
        _snake.Add(start + new Point(-3, 0));
        _snake.Add(start + new Point(-4, 0));
        _direction = Directions.Right;
        _nextDirection = Directions.Right;
        _moveInterval = .5f;
        _moveTimer = 0f;
        _score = 0;
        _gameOver = false;


        switch (level)
        {
            case levelEnum.normal:
                _moveInterval = 0.5f;
                spawnSpikeCount1 = 12;
                spawnSpikeCount2 = 20;
                break;
            case levelEnum.hard:
                _moveInterval = 0.4f;
                spawnSpikeCount1 = 8;
                spawnSpikeCount2 = 14;
                break;
            default:
                _moveInterval = 0.5f;
                break;
        }
        SpawnFood();
    }
    #endregion

    #region spawn spike
    void SpawnSpike()
    {
        Point pos;
        do
        {
            pos = new Point(_rand.Next(1, Columns - 1), _rand.Next(1, Rows - 1));
        } while (IsOccupied(pos));

        _spike = new SpikeItem(pos);
        _spikeslist = new SpikeItem[] { _spike };
        Console.WriteLine("Spawning spike" + pos);
    }

    void SpawnSpikes()
    {
        Point pos;
        Point pos2;
        Point pos3;
        do
        {
            pos = new Point(_rand.Next(1, Columns - 1), _rand.Next(1, Rows - 1));
            pos2 = pos + new Point(1, 0);
            pos3 = pos + new Point(0, 1);
        } while (IsOccupied(pos) || IsOccupied(pos2) || IsOccupied(pos3));

        // _spike = new SpikeItem(pos);
        _spikeslist = new SpikeItem[] { _spike, new SpikeItem(pos), new SpikeItem(pos2), new SpikeItem(pos3) };
        Console.WriteLine("Spawning spike" + pos);
    }
    #endregion

    #region spawn food
    private void SpawnFood()
    {
        float randomInt = (float)_rand.NextDouble();
        FoodType type;
        switch (level)
        {
            case levelEnum.normal:
                if (randomInt < 0.4f)
                    type = FoodType.Apple;
                else if (randomInt < 0.75f)
                    type = FoodType.Banana;
                else
                    type = FoodType.Blueberry;
                if (isGameJustStarted)
                {
                    type = FoodType.Apple;
                    isGameJustStarted = false;
                }
                break;
            case levelEnum.hard:
                if (randomInt < 0.6f)
                    type = FoodType.Banana;
                else
                    type = FoodType.Blueberry;
                break;
            default:
                type = FoodType.Apple;
                break;
        }

        Point pos;

        do
        {
            pos = new Point(_rand.Next(1, Columns - 1), _rand.Next(1, Rows - 1));
        } while (IsOccupied(pos));

        switch (type)
        {
            case FoodType.Apple:
                Console.WriteLine("Spawned an apple at " + pos);
                _food = new FoodItem(pos, type, 10, Color.Red);
                break;
            case FoodType.Banana:
                Console.WriteLine("Spawned an apple at " + pos);

                _food = new FoodItem(pos, type, 15, Color.Yellow);
                break;
            case FoodType.Blueberry:
                Console.WriteLine("Spawned an apple at " + pos);

                _food = new FoodItem(pos, type, 20, Color.Blue);
                break;
            default:
                _food = new FoodItem(pos, FoodType.Apple, 10, Color.Red);
                break;
        }
    }
    #endregion

    #region helper methods

    private bool IsOccupied(Point pos)
    {
        foreach (var p in _snake)
        {
            if (p == pos) return true;
        }
        return false;
    }

    private Rectangle ToRect(Point gridPos)
    {
        return new Rectangle(
            (int)(_boardOrigin.X + gridPos.X * TileSize),
            (int)(_boardOrigin.Y + gridPos.Y * TileSize),
            TileSize,
            TileSize);
    }
    #endregion

    #region data structures
    private readonly record struct SpikeItem(Point Position);

    private readonly record struct FoodItem(Point Position, FoodType Type, int Points, Color Color);
    private enum FoodType { Apple, Banana, Blueberry }
    private Dictionary<FoodType, int> foodSegmentsDict = new Dictionary<FoodType, int>()
    {
        { FoodType.Apple, 1 },
        { FoodType.Banana, 2 },
        { FoodType.Blueberry, 3 }
    };
    #endregion
}

public enum levelEnum { normal, hard }