using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace Template;

/// <summary>
/// Simplified GameState template with JSON save/load functionality
/// </summary>
public class GameState
{
    // Generic state data storage - use for any game values
    private Dictionary<string, object> stateData;

    // Player reference
    public Player Player { get; set; }

    // Control flags
    public bool AllowPlayerControl { get; set; } = true;

    // Current scene tracking
    public string CurrentSceneName { get; set; } = string.Empty;

    // Legacy properties for backwards compatibility (customize as needed)
    public int PlayerHealth { get; set; } = 6;
    public int PlayerMaxHealth { get; set; } = 6;
    public bool HasSword { get; set; } = false;
    public object HUD { get; set; } = null;
    public bool EdgeTransitionPending { get; set; } = false;
    public enum SpawnEdge { Left, Right, Top, Bottom, FromDoor, Door }
    public SpawnEdge? EdgeSpawnSide { get; set; }
    public Vector2 PlayerPositionBeforeTransition { get; set; }

    public GameState()
    {
        stateData = new Dictionary<string, object>();

        // Initialize default values
        SetState("Coins", 0);
    }

    #region State Management
    /// <summary>
    /// Set any state value by key
    /// </summary>
    public void SetState(string key, object value)
    {
        stateData[key] = value;
    }

    /// <summary>
    /// Get state value by key, returns default if not found
    /// </summary>
    public TYPE GetState<TYPE>(string key, TYPE defaultValue = default)
    {
        if (stateData.TryGetValue(key, out var value) && value is TYPE typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// Check if state key exists
    /// </summary>
    public bool HasState(string key)
    {
        return stateData.ContainsKey(key);
    }

    /// <summary>
    /// Remove state key
    /// </summary>
    public void RemoveState(string key)
    {
        stateData.Remove(key);
    }

    /// <summary>
    /// Clear all state data
    /// </summary>
    public void ClearAllState()
    {
        stateData.Clear();
    }
    #endregion

    #region transition (unused)
    public void QueueEdgeTransition(SpawnEdge edge, Vector2 position)
    {
        EdgeTransitionPending = true;
        EdgeSpawnSide = edge;
        PlayerPositionBeforeTransition = position;
    }

    public void ClearEdgeTransition()
    {
        EdgeTransitionPending = false;
        EdgeSpawnSide = null;
    }
    #endregion

    #region Item collection tracking (unused)
    private readonly Dictionary<string, HashSet<Point>> _collectedItemsByScene = new();

    public void MarkItemCollected(string sceneName, Point gridPosition)
    {
        if (!_collectedItemsByScene.ContainsKey(sceneName))
            _collectedItemsByScene[sceneName] = new HashSet<Point>();
        _collectedItemsByScene[sceneName].Add(gridPosition);
    }

    public bool IsItemCollected(string sceneName, Point gridPosition)
    {
        return _collectedItemsByScene.ContainsKey(sceneName) &&
               _collectedItemsByScene[sceneName].Contains(gridPosition);
    }

    public void PickUpItem(object item)
    {
        // Stub for item pickup - customize as needed
    }
    #endregion

    #region Save/Load System
    /// <summary>
    /// Save game state to JSON file
    /// </summary>
    public bool SaveToFile(string fileName = "savegame.json")
    {
        try
        {
            var saveData = new SaveData
            {
                StateData = new Dictionary<string, object>(stateData),
                CurrentSceneName = CurrentSceneName,
                SaveTimestamp = DateTime.Now
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(saveData, options);
            File.WriteAllText(fileName, json);

            Console.WriteLine($"Game saved to {fileName}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save game: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load game state from JSON file
    /// </summary>
    public bool LoadFromFile(string fileName = "savegame.json")
    {
        try
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Save file {fileName} not found");
                return false;
            }

            string json = File.ReadAllText(fileName);
            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };

            var saveData = JsonSerializer.Deserialize<SaveData>(json, options);

            if (saveData != null)
            {
                stateData = new Dictionary<string, object>(saveData.StateData);
                CurrentSceneName = saveData.CurrentSceneName;

                Console.WriteLine($"Game loaded from {fileName}");
                Console.WriteLine($"Save date: {saveData.SaveTimestamp}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load game: {ex.Message}");
            return false;
        }
    }
    #endregion
}

/// <summary>
/// Data structure for JSON serialization
/// </summary>
#region SaveData
public class SaveData
{
    public Dictionary<string, object> StateData { get; set; }
    public string CurrentSceneName { get; set; }
    public DateTime SaveTimestamp { get; set; }
}
#endregion
