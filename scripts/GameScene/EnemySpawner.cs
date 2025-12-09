using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public enum MapArea
{
    Area01,
    Area02,
    Area03,
    Area11,
    Area12,
    Area13
}

public enum SpawnType
{
    None,
    Random,
    Side
}

public static class EnemySpawner
{
    private static Dictionary<MapArea, string> EnemyName = new Dictionary<MapArea, string>()
    {
        { MapArea.Area01, "Spider" },
        { MapArea.Area02, "Shooter" },
        { MapArea.Area03, "Shooter"},
        { MapArea.Area11, "Spider" },
        { MapArea.Area12, "" },
        { MapArea.Area13, "Shooter" },
    };

    private static Dictionary<MapArea, SpawnType> EnemySpawnType = new Dictionary<MapArea, SpawnType>()
    {
        { MapArea.Area01, SpawnType.Random },
        { MapArea.Area02, SpawnType.Random },
        { MapArea.Area03, SpawnType.Side },
        { MapArea.Area11, SpawnType.Random },
        { MapArea.Area12, SpawnType.None },
        { MapArea.Area13, SpawnType.Side }
    };

    // Number of enemies to spawn per map
    private static Dictionary<SpawnType, int> SpawnCount = new Dictionary<SpawnType, int>()
    {
        { SpawnType.None, 0 },
        { SpawnType.Random, 3 },
        { SpawnType.Side, 4 }
    };

    public static async void SpawnEnemiesForMap(Scene scene, string levelName)
    {
        // Parse level name to get MapArea (e.g., "Zelda01" -> Area01)
        MapArea? area = ParseLevelName(levelName);
        if (!area.HasValue) return;

        if (!EnemyName.ContainsKey(area.Value) || !EnemySpawnType.ContainsKey(area.Value))
            return;

        string enemyName = EnemyName[area.Value];
        SpawnType spawnType = EnemySpawnType[area.Value];

        if (spawnType == SpawnType.None || string.IsNullOrEmpty(enemyName))
            return;

        int count = SpawnCount[spawnType];

        // Wait 5 seconds before spawning enemies so player has time to prepare
        await Task.Delay(1000);

        switch (spawnType)
        {
            case SpawnType.Random:
                await SpawnRandomEnemies(scene, enemyName, count);
                break;
            case SpawnType.Side:
                SpawnSideEnemies(scene, enemyName, count);
                break;
        }
    }


    private static MapArea? ParseLevelName(string levelName)
    {
        // Parse "Zelda01" -> Area01, "Zelda13" -> Area13, etc.
        if (levelName.Length < 7 || !levelName.StartsWith("Zelda"))
            return null;

        string areaCode = levelName.Substring(5, 2);
        if (Enum.TryParse<MapArea>("Area" + areaCode, out MapArea area))
        {
            return area;
        }
        return null;
    }

    private static async Task SpawnRandomEnemies(Scene scene, string enemyName, int count)
    {
        Random rand = new Random();

        // Define spawn area (avoid HUD at top and edges)
        float minX = Grid.TileSize * 2;
        float maxX = Scene.GameSceneSize.X - Grid.TileSize * 2;
        float minY = Scene.SceneOffset.Y + Grid.TileSize * 2;
        float maxY = Scene.SceneOffset.Y + Scene.GameSceneSize.Y - Grid.TileSize * 2;

        for (int i = 0; i < count; i++)
        {
            Vector2 position = new Vector2(
                (float)(rand.NextDouble() * (maxX - minX) + minX),
                (float)(rand.NextDouble() * (maxY - minY) + minY)
            );

            // Snap to grid
            Point gridPos = Grid.GetGridPositionFromPixelPosition(position - Scene.SceneOffset);
            position = Grid.GetPixelPositionFromGridPosition(gridPos) + Scene.SceneOffset;

            // Check if position is valid (not solid)
            if (scene.Grid != null && !scene.Grid.IsTileSolid(position))
            {
                // Spawn enemy immediately (no spawn animation)
                Enemy enemy = Enemy.MakeEnemy(position, Art.Enemy, enemyName);
                scene.AddEntity(enemy);
            }
            else
            {
                // Try again with a different position
                i--;
            }
        }
    }

    private static Dictionary<MapArea, Directions[]> spawnSides = new Dictionary<MapArea, Directions[]>()
    {
        { MapArea.Area03, new Directions[] { Directions.Down, Directions.Right } },
        { MapArea.Area13, new Directions[] { Directions.Up, Directions.Right} },
    };

    private static void SpawnSideEnemies(Scene scene, string enemyName, int count)
    {
        // Spawn enemies along the sides of the map at random gaps along the chosen side.
        Random rand = new Random();

        // playable ranges (leave a margin from edges and HUD)
        float minX = Grid.TileSize;
        float maxX = Scene.GameSceneSize.X;
        float minY = Scene.SceneOffset.Y;
        float maxY = Scene.SceneOffset.Y + Scene.GameSceneSize.Y;

        // compute grid-aligned ranges so we pick exact grid cells on the edges
        Point gridMin = Grid.GetGridPositionFromPixelPosition(new Vector2(minX, minY) - Scene.SceneOffset);
        Point gridMax = Grid.GetGridPositionFromPixelPosition(new Vector2(maxX, maxY) - Scene.SceneOffset);

        // Determine which sides are allowed for this map
        List<Directions> allowedSides = new List<Directions>();
        MapArea? area = ParseLevelName(scene.LevelName ?? "");
        if (area.HasValue && spawnSides.ContainsKey(area.Value))
        {
            allowedSides.AddRange(spawnSides[area.Value]);
        }
        else
        {
            allowedSides.AddRange(new Directions[] { Directions.Left, Directions.Right, Directions.Up, Directions.Down });
        }

        int spawned = 0;
        int attempts = 0;
        while (spawned < count && attempts < count * 10)
        {
            Console.WriteLine(count);
            attempts++;
            // pick a random allowed side
            Directions side = allowedSides[rand.Next(allowedSides.Count)];

            // pick a random grid cell along the chosen side (so spawn aligns to top/side grid)
            Point chosenGrid = new Point();
            switch (side)
            {
                case Directions.Left:
                    chosenGrid.X = gridMin.X;
                    chosenGrid.Y = rand.Next(gridMin.Y, gridMax.Y + 1);
                    break;
                case Directions.Right:
                    chosenGrid.X = gridMax.X;
                    chosenGrid.Y = rand.Next(gridMin.Y, gridMax.Y + 1);
                    break;
                case Directions.Up:
                    chosenGrid.Y = gridMin.Y;
                    chosenGrid.X = rand.Next(gridMin.X, gridMax.X + 1);
                    break;
                case Directions.Down:
                    chosenGrid.Y = gridMax.Y;
                    chosenGrid.X = rand.Next(gridMin.X, gridMax.X + 1);
                    break;
            }


            Vector2 position = Grid.GetPixelPositionFromGridPosition(chosenGrid) + Scene.SceneOffset;

            // Validate
            if (scene.Grid != null && scene.Grid.IsTileSolid(position))
            {
                continue; // try another
            }

            // Spawn enemy immediately at the chosen grid cell (no slide-in)
            Enemy enemy = Enemy.MakeEnemy(position, Art.Enemy, enemyName);
            scene.AddEntity(enemy);

            spawned++;
        }
    }
}