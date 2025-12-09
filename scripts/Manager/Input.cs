using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Template;

public enum Action
{
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    Talk,
    Start,
    Select,
    ActionA,
    ActionB
}

public class Input
{
    public static Dictionary<Action, List<Keys>> actionKeys = new Dictionary<Action, List<Keys>>()
    {
        { Action.MoveLeft, new List<Keys>{Keys.Left, Keys.A} },
        { Action.MoveRight, new List<Keys>{Keys.Right, Keys.D} },
        { Action.MoveUp, new List<Keys>{Keys.Up, Keys.W} },
        { Action.MoveDown, new List<Keys>{Keys.Down, Keys.S} },
        { Action.Talk, new List<Keys>{Keys.Space} },
        { Action.Start, new List<Keys>{Keys.Enter} },
        { Action.Select, new List<Keys>{Keys.V} },
        { Action.ActionA, new List<Keys>{Keys.Z} },
        { Action.ActionB, new List<Keys>{Keys.X} },
    };

    private static KeyboardState _currentKey;
    private static KeyboardState _prevKey;

    private static MouseState _currentMouse;
    private static MouseState _prevMouse;

    public static void Update()
    {
        _prevKey = _currentKey;
        _currentKey = Keyboard.GetState();

        _prevMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
    }

    #region Keyboard Input
    public bool IsActionPressed(Action action)
    {
        if (actionKeys.ContainsKey(action) == false)
            return false;
        foreach (var key in actionKeys[action])
        {
            if (IsKeyPressed(key))
                return true;
        }
        return false;
    }

    bool IsKeyPressed(Keys key)
    {
        return _currentKey.IsKeyDown(key) && _prevKey.IsKeyUp(key);
    }

    public bool IsActionDown(Action action)
    {
        if (actionKeys.ContainsKey(action) == false)
            return false;
        foreach (var key in actionKeys[action])
        {
            if (IsKeyDown(key))
                return true;
        }
        return false;
    }

    bool IsKeyDown(Keys key)
    {
        return _currentKey.IsKeyDown(key);
    }
    #endregion

    #region Mouse Input

    /// <summary>
    /// Returns current mouse position
    /// </summary>
    public static Vector2 GetMousePosition()
    {
        return new Vector2(_currentMouse.X, _currentMouse.Y);
    }

    /// <summary>
    /// Returns true if left mouse button was clicked this frame
    /// </summary>
    public static bool IsLeftMouseClick()
    {
        return _currentMouse.LeftButton == ButtonState.Pressed &&
               _prevMouse.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Returns true if right mouse button was clicked this frame
    /// </summary>
    public static bool IsRightMouseClick()
    {
        return _currentMouse.RightButton == ButtonState.Pressed &&
               _prevMouse.RightButton == ButtonState.Released;
    }

    /// <summary>
    /// Returns true if left mouse button is being held down
    /// </summary>
    public static bool IsLeftMouseDown()
    {
        return _currentMouse.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Returns true if right mouse button is being held down
    /// </summary>
    public static bool IsRightMouseDown()
    {
        return _currentMouse.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Returns true if mouse is being dragged (left button held and position changed)
    /// </summary>
    public static bool IsMouseDragging()
    {
        return _currentMouse.LeftButton == ButtonState.Pressed &&
               (_currentMouse.X != _prevMouse.X || _currentMouse.Y != _prevMouse.Y);
    }

    /// <summary>
    /// Returns the drag delta (difference in position) while dragging
    /// </summary>
    public static Vector2 GetMouseDragDelta()
    {
        if (IsMouseDragging())
        {
            return new Vector2(
                _currentMouse.X - _prevMouse.X,
                _currentMouse.Y - _prevMouse.Y
            );
        }
        return Vector2.Zero;
    }

    /// <summary>
    /// Returns true if mouse position has changed (hovering/moving)
    /// </summary>
    public static bool IsMouseHovering()
    {
        return _currentMouse.X != _prevMouse.X || _currentMouse.Y != _prevMouse.Y;
    }

    /// <summary>
    /// Returns the previous mouse position (useful for hover effects)
    /// </summary>
    public static Vector2 GetPreviousMousePosition()
    {
        return new Vector2(_prevMouse.X, _prevMouse.Y);
    }

    #endregion
}