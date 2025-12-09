using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Template;

public class ButtonEntity : Entity
{
    private int _width;
    private int _height;
    private string _text;
    private SpriteFont _font;

    private Color _backgroundColor;
    private Color _hoverColor;
    private Color _textColor;

    private bool _isHovered;
    private bool _wasMouseDown;

    private System.Action _onClick;

    public ButtonEntity(
        Vector2 position,
        int width,
        int height,
        string text = "",
        Color? backgroundColor = null,
        Color? hoverColor = null,
        Color? textColor = null
    )
        : base(position)
    {
        _width = width;
        _height = height;
        _text = text ?? string.Empty;
        _font = AssetManager.font;

        _backgroundColor = backgroundColor ?? new Color(80, 80, 80);
        _hoverColor = hoverColor ?? new Color(110, 110, 110);
        _textColor = textColor ?? Color.White;

        _rect_offset = new Point(0, 0);
        _rect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);

        _label = "button";
    }

    public void SetOnClick(System.Action onClick)
    {
        _onClick = onClick;
    }

    public override void Update()
    {
        var mouse = Mouse.GetState();
        var mousePoint = new Point(mouse.X, mouse.Y);
        _isHovered = _rect.Contains(mousePoint);

        bool isMouseDown = mouse.LeftButton == ButtonState.Pressed;

        if (_isHovered && isMouseDown && !_wasMouseDown)
        {
            if (_onClick != null)
            {
                _onClick();
            }
        }

        _wasMouseDown = isMouseDown;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Background rectangle
        var bgColor = _isHovered ? _hoverColor : _backgroundColor;
        spriteBatch.Draw(AssetManager.GetTexture(Art.Pixel), _rect, bgColor);

        // Optional text centered
        if (!string.IsNullOrEmpty(_text) && _font != null)
        {
            Vector2 textSize = _font.MeasureString(_text);
            Vector2 textPos = new Vector2(
                _rect.X + (_rect.Width - textSize.X) / 2f,
                _rect.Y + (_rect.Height - textSize.Y) / 2f
            );
            spriteBatch.DrawString(_font, _text, textPos, _textColor);
        }
    }

    public void MoveTo(Vector2 newPosition)
    {
        _position = newPosition;
        _rect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
    }
}

