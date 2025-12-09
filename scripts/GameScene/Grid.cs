using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Template;

public class Grid
{
    public const int TileSize = 64;
    private Dictionary<Point, Tile> tiles;
    public Vector2 Offset { get; set; } = Vector2.Zero;

    public Grid()
    {
        tiles = new Dictionary<Point, Tile>();
    }

    public void SetTile(Point position, TileType tileType)
    {
        Tile tile = new Tile(position, tileType);
        tiles[position] = tile;
    }

    public void RemoveTile(Point position)
    {
        tiles.Remove(position);
    }

    bool IsGridPositionWithinGridSize(Point gridPosition)
    {
        return tiles.ContainsKey(gridPosition);
    }

    public Tile GetTile(Point gridPosition)
    {
        if (IsGridPositionWithinGridSize(gridPosition) == false) return null;
        return tiles[gridPosition];
    }

    public Tile GetTile(Vector2 pixelPosition)
    {
        return GetTile(GetGridPositionFromPixelPosition(pixelPosition - Offset));
    }


    public Tile GetTileAtGridPosition(Vector2 pixelPosition)
    {
        return GetTile(GetGridPositionFromPixelPosition(pixelPosition - Offset));
    }

    public bool IsTileSolid(Point gridPosition)
    {
        Tile tile = GetTile(gridPosition);
        if (tile == null) return false;
        return Tile.tileSolid[tile.Type];
    }

    public bool IsTileSolid(Vector2 pixelPosition)
    {
        return IsTileSolid(GetGridPositionFromPixelPosition(pixelPosition - Offset));
    }

    public bool IsPixelSolid(Vector2 worldPixel)
    {
        Vector2 pixelPos = worldPixel - Offset;
        Point gridPos = GetGridPositionFromPixelPosition(pixelPos);

        Tile tile = GetTile(gridPos);
        if (tile == null) return false;

        // Only tiles marked solid are candidates for collision
        if (!Tile.tileSolid.ContainsKey(tile.Type) || Tile.tileSolid[tile.Type] == false)
        {
            return false;
        }

        // Local coordinates within the tile
        int localX = (int)(pixelPos.X - (gridPos.X * TileSize));
        int localY = (int)(pixelPos.Y - (gridPos.Y * TileSize));

        // Safety clamp in case of rounding
        if (localX < 0 || localY < 0 || localX >= TileSize || localY >= TileSize)
        {
            return false;
        }

        return Tile.IsPixelSolid(tile.Type, localX, localY);
    }
    public bool IsPixelSolidByBackgroundColor(Vector2 worldPixel)
    {
        // Transform world pixel into grid/tile space by removing visual offset
        Vector2 pixelPos = worldPixel - Offset;
        Point gridPos = GetGridPositionFromPixelPosition(pixelPos);

        Tile tile = GetTile(gridPos);
        if (tile == null) return false;

        // Only tiles marked solid are candidates for collision
        if (!Tile.tileSolid.ContainsKey(tile.Type) || Tile.tileSolid[tile.Type] == false)
        {
            return false;
        }

        // Local coordinates within the tile
        int localX = (int)(pixelPos.X - (gridPos.X * TileSize));
        int localY = (int)(pixelPos.Y - (gridPos.Y * TileSize));

        // Safety clamp in case of rounding
        if (localX < 0 || localY < 0 || localX >= TileSize || localY >= TileSize)
        {
            return false;
        }
        return !Tile.IsPixelBackgroundColor(tile.Type, localX, localY);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var tile in tiles.Values)
        {
            tile.Draw(spriteBatch, Offset);
        }
    }

    public static Vector2 GetPixelPositionFromGridPosition(Point gridPos)
    {
        return new Vector2(gridPos.X * TileSize, gridPos.Y * TileSize);
    }

    public static Point GetGridPositionFromPixelPosition(Vector2 pixelPos)
    {
        Point gridPos = new Point(0, 0);
        gridPos.X = (int)Math.Floor(pixelPos.X / TileSize);
        gridPos.Y = (int)Math.Floor(pixelPos.Y / TileSize);
        return gridPos;
    }
}
