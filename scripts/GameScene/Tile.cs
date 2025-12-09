using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Template;

public enum TileType
{
    Empty,
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
    Underworld,
    UnderworldTop
}

public class Tile
{
    private static Dictionary<TileType, bool[,]> _solidMasks = new Dictionary<TileType, bool[,]>();
    private static readonly byte AlphaThreshold = 10; // treat pixels with alpha > threshold as solid
    private static Dictionary<TileType, bool[,]> _backgroundColorMasks = new Dictionary<TileType, bool[,]>();

    static Dictionary<TileType, Color> TileColors = new Dictionary<TileType, Color>()
    {
        { TileType.Empty, Color.Transparent },
        { TileType.Wall, Color.White },
        { TileType.WallTwo, Color.White },
        { TileType.Tree, Color.White },
        { TileType.TopMid, Color.White },
        { TileType.TopLeft, Color.White },
        { TileType.TopRight, Color.White },
        { TileType.BottomMid, Color.White },
        { TileType.BottomLeft, Color.White },
        { TileType.BottomRight, Color.White },
        { TileType.Cave, Color.White },
        { TileType.Underworld, Color.White },
        { TileType.UnderworldTop, Color.White },
    };

    static Dictionary<TileType, Texture2D> TileArt = new Dictionary<TileType, Texture2D>()
    {
        { TileType.Empty, AssetManager.GetTexture(Art.Pixel) },
        { TileType.Wall, AssetManager.GetTileset(TilesetArt.Wall) },
        { TileType.WallTwo, AssetManager.GetTileset(TilesetArt.WallTwo) },
        { TileType.Tree, AssetManager.GetTileset(TilesetArt.Tree) },
        { TileType.TopMid, AssetManager.GetTileset(TilesetArt.TopMid) },
        { TileType.TopLeft, AssetManager.GetTileset(TilesetArt.TopLeft) },
        { TileType.TopRight, AssetManager.GetTileset(TilesetArt.TopRight) },
        { TileType.BottomMid, AssetManager.GetTileset(TilesetArt.BottomMid) },
        { TileType.BottomLeft, AssetManager.GetTileset(TilesetArt.BottomLeft) },
        { TileType.BottomRight, AssetManager.GetTileset(TilesetArt.BottomRight) },
        { TileType.Underworld, AssetManager.GetTileset(TilesetArt.underworld) },
        { TileType.UnderworldTop, AssetManager.GetTileset(TilesetArt.underworldTop) },
        { TileType.Cave, AssetManager.GetTileset(TilesetArt.Cave) },
    };

    private static void EnsureMaskBuilt(TileType type)
    {
        if (_solidMasks.ContainsKey(type)) return;

        var tex = TileArt[type];
        int w = tex.Width;
        int h = tex.Height;
        var data = new Color[w * h];
        tex.GetData(data);
        var mask = new bool[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var c = data[y * w + x];
                mask[x, y] = c.A > AlphaThreshold;
            }
        }
        _solidMasks[type] = mask;
    }

    public static bool IsPixelSolid(TileType type, int localX, int localY)
    {
        EnsureMaskBuilt(type);
        var mask = _solidMasks[type];

        if (localX < 0 || localY < 0 || localX >= mask.GetLength(0) || localY >= mask.GetLength(1))
            return false;
        return mask[localX, localY];
    }

    private static void EnsureBackgroundMaskBuilt(TileType type)
    {
        if (_backgroundColorMasks.ContainsKey(type)) return;

        // get pixel color data of the texture
        var gridTexture = TileArt[type];
        int width = gridTexture.Width;
        int height = gridTexture.Height;
        var data = new Color[width * height]; //array of all the colors of each pixel of grid
        gridTexture.GetData(data); // get all the color from data

        // a boolean array for each pixel
        var mask = new bool[width, height];

        var target = Game1.ColorBackground;

        //check for each pixel
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixelColor = data[y * width + x];
                mask[x, y] = pixelColor.R == target.R && pixelColor.G == target.G && pixelColor.B == target.B && pixelColor.A == target.A;
            }
        }

        //store in the dictionary
        _backgroundColorMasks[type] = mask;

    }

    public static bool IsPixelBackgroundColor(TileType type, int localX, int localY)
    {
        EnsureBackgroundMaskBuilt(type);
        var mask = _backgroundColorMasks[type];
        if (localX < 0 || localY < 0 || localX >= mask.GetLength(0) || localY >= mask.GetLength(1))
            return false;
        return mask[localX, localY];
    }

    public static Dictionary<TileType, bool> tileSolid = new Dictionary<TileType, bool>()
    {
        {TileType.Empty, false },
        {TileType.Wall, true},
        {TileType.WallTwo, true},
        {TileType.Tree, true},
        {TileType.TopMid, true},
        {TileType.TopLeft, true},
        {TileType.TopRight, true},
        {TileType.BottomMid, true},
        {TileType.BottomLeft, true},
        {TileType.BottomRight, true},
        {TileType.Underworld, true},
        {TileType.UnderworldTop, true},
        {TileType.Cave, true},
    };

    public static Dictionary<string, TileType> tileSymbols = new Dictionary<string, TileType>()
    {
        { "#_@", TileType.Wall },
        { "#_@/", TileType.WallTwo },
        { "#_tree", TileType.Tree },
        { "#_#+", TileType.TopMid },
        { "#_<+", TileType.TopLeft },
        { "#_>+", TileType.TopRight },
        { "#_#-", TileType.BottomMid },
        { "#_<-", TileType.BottomLeft },
        { "#_>-", TileType.BottomRight },
        { "#_underworld", TileType.Underworld },
        { "#_underworld+", TileType.UnderworldTop },
        { "#_cave", TileType.Cave },
    };

    protected Point _gridPosition;
    protected TileType _tileType;
    public Point GridPosition => _gridPosition;
    public Point PixelPosition => new Point(_gridPosition.X * Grid.TileSize, _gridPosition.Y * Grid.TileSize);
    public TileType Type => _tileType;

    public Tile(Point GridPosition, TileType tileType)
    {
        _gridPosition = GridPosition;
        _tileType = tileType;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 offset)
    {
        spriteBatch.Draw(
            TileArt[_tileType],
            new Rectangle(
                (int)(PixelPosition.X + offset.X),
                (int)(PixelPosition.Y + offset.Y),
                Grid.TileSize,
                Grid.TileSize
            ),
            TileColors[_tileType]
        );
    }
}