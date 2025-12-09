using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Template;

// Legacy enums for backwards compatibility
public enum HeartState { Empty, Half, Full }
public enum Item { None, Sword, Coin }

public enum Art
{
    apple,
    banana,
    blueberry,
    snake,
    Pixel,
    Enemy,
    Player,
    Exit,
    Coin,
    StartScreen,
    HUDlarge,
    Cave,
    sword,
    win,
    NPCLarge,
}

public enum Font
{
    // Jaro,
    // JaroTextbox
}

public enum TilesetArt
{
    Wall,
    WallTwo,
    Tree,
    TopMid,
    TopLeft,
    TopRight,
    BottomMid,
    BottomLeft,
    BottomRight,
    Cave,
    underworld,
    underworldTop,
}

public static class AssetManager
{
    const int TILE_SIZE = 16 * 4;
    const int OFFSET_X = 0;  // left margin
    const int OFFSET_Y = 0;  // top margin

    private static Dictionary<Art, Texture2D> _textures = new Dictionary<Art, Texture2D>();
    public static Dictionary<Font, SpriteFont> _fonts = new Dictionary<Font, SpriteFont>();
    public static Dictionary<TilesetArt, Texture2D> _tilesets = new Dictionary<TilesetArt, Texture2D>();

    public static Dictionary<(Directions, PlayerState), Texture2D[]> PlayerAnimations =
        new Dictionary<(Directions, PlayerState), Texture2D[]>();

    public static Dictionary<Directions, Texture2D[]> EnemyShooterAnimations =
        new Dictionary<Directions, Texture2D[]>();
    public static Texture2D ShooterBullet;

    public static Dictionary<Directions, Texture2D> EnemySpiderArt =
    new Dictionary<Directions, Texture2D>();

    // Re-added for backwards compatibility
    public static Dictionary<HeartState, Texture2D> Heart =
    new Dictionary<HeartState, Texture2D>();

    public static Dictionary<Directions, Texture2D> SwordDirection =
    new Dictionary<Directions, Texture2D>();
    public static SpriteFont font;

    public static Texture2D[] fireAnimationFrames;
    public static Texture2D[] SpawnFrames;
    public static Texture2D[] DeathFrames;

    public static void LoadContent(ContentManager content)
    {
        foreach (Art art in Enum.GetValues(typeof(Art)))
        {
            string art_name = Enum.GetName(typeof(Art), art);
            try
            {
                Texture2D texture = content.Load<Texture2D>(art_name);
                _textures.Add(art, texture);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: failed to load art '{art_name}': {e.Message}");
            }
        }

        font = content.Load<SpriteFont>("Font");

        foreach (Font font in Enum.GetValues(typeof(Font)))
        {
            string font_name = Enum.GetName(typeof(Font), font);
            try
            {
                SpriteFont myFont = content.Load<SpriteFont>(font_name);
                _fonts.Add(font, myFont);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: failed to load font '{font_name}': {e.Message}");
            }
        }

        #region Tileset Loading
        Texture2D tileSheet = content.Load<Texture2D>("TilesetLarge");
        _tilesets[TilesetArt.Wall] = GetTile(tileSheet, 1, 1, 0);
        _tilesets[TilesetArt.WallTwo] = GetTile(tileSheet, 2, 1, 0);
        _tilesets[TilesetArt.Tree] = GetTile(tileSheet, 3, 1, 0);
        _tilesets[TilesetArt.TopLeft] = GetTile(tileSheet, 4, 1, 0);
        _tilesets[TilesetArt.TopMid] = GetTile(tileSheet, 5, 1, 0);
        _tilesets[TilesetArt.TopRight] = GetTile(tileSheet, 6, 1, 0);
        _tilesets[TilesetArt.BottomMid] = GetTile(tileSheet, 8, 1, 0);
        _tilesets[TilesetArt.BottomLeft] = GetTile(tileSheet, 7, 1, 0);
        _tilesets[TilesetArt.BottomRight] = GetTile(tileSheet, 9, 1, 0);
        _tilesets[TilesetArt.Cave] = GetTile(tileSheet, 10, 1, 0);
        _tilesets[TilesetArt.underworld] = GetTile(tileSheet, 11, 1, 0);
        _tilesets[TilesetArt.underworldTop] = GetTile(tileSheet, 12, 1, 0);
        #endregion

        #region PlayerWalking
        Texture2D playerSheet = content.Load<Texture2D>("PlayerMoveLarge");

        PlayerAnimations[(Directions.Down, PlayerState.Walking)] = new Texture2D[]
        {
            GetTile(playerSheet, 1, 1, 4),  // frame 1
            GetTile(playerSheet, 2, 1, 4),  // frame 2
        };
        PlayerAnimations[(Directions.Up, PlayerState.Walking)] = new Texture2D[]
        {
            GetTile(playerSheet, 5, 1, 4),
            GetTile(playerSheet, 6, 1, 4),
        };
        PlayerAnimations[(Directions.Left, PlayerState.Walking)] = new Texture2D[]
        {
            GetTile(playerSheet, 3, 1,4).FlipHorizontally(),
            GetTile(playerSheet, 4, 1,4).FlipHorizontally(),
        };
        PlayerAnimations[(Directions.Right, PlayerState.Walking)] = new Texture2D[]
        {
            GetTile(playerSheet, 3, 1,4),
            GetTile(playerSheet, 4, 1,4),
        };
        #endregion

        #region PlayerDamaged
        PlayerAnimations[(Directions.Down, PlayerState.Damaged)] = new Texture2D[]
        {
            GetTile(playerSheet, 2, 1, 4),
            GetTile(playerSheet, 2, 1, 4).ChangeColor(new Color(252, 216, 168), new Color(0, 128, 136)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 0)).ChangeColor(new Color(200, 76, 12), new Color(216, 40, 0)),
            GetTile(playerSheet, 2, 1, 4).ChangeColor(new Color(252, 216, 168), new Color(252, 152, 56)).ChangeColor(new Color(128, 208, 16), new Color(216, 40, 0)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),
            GetTile(playerSheet, 2, 1, 4).ChangeColor(new Color(252, 216, 168), new Color(92, 148, 252)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 168)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),
        };
        PlayerAnimations[(Directions.Up, PlayerState.Damaged)] = new Texture2D[]
        {
            GetTile(playerSheet, 5, 1, 4),
            GetTile(playerSheet, 5, 1, 4).ChangeColor(new Color(252, 216, 168), new Color(0, 128, 136)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 0)).ChangeColor(new Color(200, 76, 12), new Color(216, 40, 0)),
            GetTile(playerSheet, 5, 1, 4).ChangeColor(new Color(252, 216, 168), new Color(252, 152, 56)).ChangeColor(new Color(128, 208, 16), new Color(216, 40, 0)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),
            GetTile(playerSheet, 5, 1, 4).ChangeColor(new Color(252, 216, 168), new Color(92, 148, 252)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 168)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),

        };
        PlayerAnimations[(Directions.Left, PlayerState.Damaged)] = new Texture2D[]
        {
            GetTile(playerSheet, 3, 1,4).FlipHorizontally(),
            GetTile(playerSheet, 3, 1,4).FlipHorizontally().ChangeColor(new Color(252, 216, 168), new Color(0, 128, 136)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 0)).ChangeColor(new Color(200, 76, 12), new Color(216, 40, 0)),
            GetTile(playerSheet, 3, 1,4).FlipHorizontally().ChangeColor(new Color(252, 216, 168), new Color(252, 152, 56)).ChangeColor(new Color(128, 208, 16), new Color(216, 40, 0)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),
            GetTile(playerSheet, 3, 1,4).FlipHorizontally().ChangeColor(new Color(252, 216, 168), new Color(92, 148, 252)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 168)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),
        };
        PlayerAnimations[(Directions.Right, PlayerState.Damaged)] = new Texture2D[]
        {
            GetTile(playerSheet, 3, 1,4),
            GetTile(playerSheet, 3, 1,4).ChangeColor(new Color(252, 216, 168), new Color(0, 128, 136)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 0)).ChangeColor(new Color(200, 76, 12), new Color(216, 40, 0)),
            GetTile(playerSheet, 3, 1,4).ChangeColor(new Color(252, 216, 168), new Color(252, 152, 56)).ChangeColor(new Color(128, 208, 16), new Color(216, 40, 0)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),
            GetTile(playerSheet, 3, 1,4).ChangeColor(new Color(252, 216, 168), new Color(92, 148, 252)).ChangeColor(new Color(128, 208, 16), new Color(0, 0, 168)).ChangeColor(new Color(200, 76, 12), new Color(255, 255, 255)),
        };
        #endregion

        #region PlayerAttacking
        Texture2D playerAttackUpSheet = content.Load<Texture2D>("playerAttackUpLarge");
        Texture2D playerAttackDownSheet = content.Load<Texture2D>("playerAttackDownLarge");
        Texture2D playerAttackRightSheet = content.Load<Texture2D>("playerAttackRightLarge");


        PlayerAnimations[(Directions.Up, PlayerState.Attacking)] = new Texture2D[]
        {
            GetTile(playerAttackUpSheet, 1, 1, 4, 64, 128),
            GetTile(playerAttackUpSheet, 2, 1, 4, 64, 128),
            GetTile(playerAttackUpSheet, 3, 1, 4, 64, 128),
            GetTile(playerAttackUpSheet, 4, 1, 4, 64, 128),
        };
        PlayerAnimations[(Directions.Down, PlayerState.Attacking)] = new Texture2D[]
        {
            GetTile(playerAttackDownSheet, 1, 1, 4, 64, 108),
            GetTile(playerAttackDownSheet, 2, 1, 4, 64, 108),
            GetTile(playerAttackDownSheet, 3, 1, 4, 64, 108),
            GetTile(playerAttackDownSheet, 4, 1, 4, 64, 108),
        };
        PlayerAnimations[(Directions.Left, PlayerState.Attacking)] = new Texture2D[]
        {
            GetTileTexture(playerAttackRightSheet, 0, 0, 64, 64).FlipHorizontally(),
            GetTileTexture(playerAttackRightSheet, 68, 0, 108, 64).FlipHorizontally(),
            GetTileTexture(playerAttackRightSheet, 180, 0, 92, 64).FlipHorizontally(),
            GetTileTexture(playerAttackRightSheet, 276, 0, 76, 64).FlipHorizontally(),
        };
        PlayerAnimations[(Directions.Right, PlayerState.Attacking)] = new Texture2D[]
        {
            GetTileTexture(playerAttackRightSheet, 0, 0, 64, 64),
            GetTileTexture(playerAttackRightSheet, 68, 0, 108, 64),
            GetTileTexture(playerAttackRightSheet, 180, 0, 92, 64),
            GetTileTexture(playerAttackRightSheet, 276, 0, 76, 64),
        };
        #endregion


        #region Enemy
        Texture2D enemyShooterSheet = content.Load<Texture2D>("EnemyShooterLarge");
        EnemyShooterAnimations[Directions.Down] = new Texture2D[]
        {
            GetTile(enemyShooterSheet, 1, 1, 4),  // frame 1
            GetTile(enemyShooterSheet, 2, 1, 4),  // frame 2
        };

        EnemyShooterAnimations[Directions.Up] = new Texture2D[]
        {
            GetTile(enemyShooterSheet, 1, 1, 4).FlipVertically(),
            GetTile(enemyShooterSheet, 2, 1, 4).FlipVertically(),
        };

        EnemyShooterAnimations[Directions.Left] = new Texture2D[]
        {
            GetTile(enemyShooterSheet, 3, 1, 4),
            GetTile(enemyShooterSheet, 4, 1, 4),
        };

        EnemyShooterAnimations[Directions.Right] = new Texture2D[]
        {
            GetTile(enemyShooterSheet, 3, 1, 4).FlipHorizontally(),
            GetTile(enemyShooterSheet, 4, 1, 4).FlipHorizontally(),
        };

        Console.WriteLine("Loaded enemy shooter animations.");

        ShooterBullet = content.Load<Texture2D>("Bullet");

        Texture2D enemySpiderSheet = content.Load<Texture2D>("EnemySpiderLarge");

        EnemySpiderArt[Directions.Down] = GetTile(enemySpiderSheet, 1, 1, 4);
        EnemySpiderArt[Directions.Up] = GetTile(enemySpiderSheet, 2, 1, 4);
        #endregion

        #region Heart / sword
        Texture2D heartSheet = content.Load<Texture2D>("HUDHeart");
        Heart[HeartState.Full] = GetTile(heartSheet, 3, 1, 4, 32);
        Heart[HeartState.Half] = GetTile(heartSheet, 2, 1, 4, 32);
        Heart[HeartState.Empty] = GetTile(heartSheet, 1, 1, 4, 32);

        Texture2D swordSheet = content.Load<Texture2D>("sword");
        Texture2D swordLeftSheet = content.Load<Texture2D>("swordLeft");


        SwordDirection[Directions.Up] = swordSheet;
        SwordDirection[Directions.Down] = swordSheet.FlipVertically();
        SwordDirection[Directions.Left] = swordLeftSheet;
        SwordDirection[Directions.Right] = swordLeftSheet.FlipHorizontally();

        #endregion

        #region spawn / death / fire animation

        Texture2D fireSheet = content.Load<Texture2D>("fireLarge");
        fireAnimationFrames = new Texture2D[]
        {
            GetTile(fireSheet, 1, 1, 4),
            GetTile(fireSheet, 2, 1, 4),
        };

        Texture2D SpawnSheet = content.Load<Texture2D>("SpawnLarge");
        SpawnFrames = new Texture2D[]
        {
                GetTile(SpawnSheet, 1, 1, 0),
                GetTile(SpawnSheet, 2, 1, 0),
                GetTile(SpawnSheet, 3, 1, 0),

        };


        Texture2D DeathSheet = content.Load<Texture2D>("DeathLarge");
        DeathFrames = new Texture2D[]
        {
                GetTile(DeathSheet, 1, 1, 0, 60, 64),
                GetTile(DeathSheet, 2, 1, 0, 60, 64),
                GetTile(DeathSheet, 3, 1, 0, 60, 64),
        };
        #endregion
    }

    #region Tile Extraction Methods
    private static Texture2D GetTile(Texture2D texture, int tileX, int tileY, int padding, int tileSizeX, int tileSizeY)
    {
        int x = OFFSET_X + (tileSizeX + padding) * (tileX - 1);
        int y = OFFSET_Y + (tileSizeY + padding) * (tileY - 1);
        return GetTileTexture(texture, x, y, tileSizeX, tileSizeY);
    }

    private static Texture2D GetTile(Texture2D texture, int tileX, int tileY, int padding, int tileSize)
    {
        int x = OFFSET_X + (tileSize + padding) * (tileX - 1);
        int y = OFFSET_Y + (tileSize + padding) * (tileY - 1);
        return GetTileTexture(texture, x, y, tileSize, tileSize);
    }

    private static Texture2D GetTile(Texture2D texture, int tileX, int tileY, int padding)
    {
        int x = OFFSET_X + (TILE_SIZE + padding) * (tileX - 1);
        int y = OFFSET_Y + (TILE_SIZE + padding) * (tileY - 1);
        return GetTileTexture(texture, x, y, TILE_SIZE, TILE_SIZE);
    }

    private static Texture2D GetTileTexture(Texture2D texture, int x, int y, int width, int height)
    {
        Color[] data = new Color[width * height];
        texture.GetData(0, new Rectangle(x, y, width, height), data, 0, data.Length);

        Texture2D subTexture = new Texture2D(texture.GraphicsDevice, width, height);
        subTexture.SetData(data);

        return subTexture;
    }

    #endregion

    public static Texture2D GetTexture(Art art)
    {
        return _textures[art];
    }

    public static SpriteFont GetFont(Font font)
    {
        return _fonts[font];
    }

    public static Texture2D GetTileset(TilesetArt art)
    {
        return _tilesets[art];
    }
}
