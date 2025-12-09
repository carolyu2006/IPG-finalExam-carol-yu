using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public class TextEntity : Entity
{
    protected string _text;
    public TextEntity(Vector2 position, string text) : base(position)
    {
        _text = text;
        _label = "text";
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(AssetManager.font, _text, Position, Color.White);
    }

    public virtual void SetText(string new_text)
    {
        _text = new_text;
    }
}

