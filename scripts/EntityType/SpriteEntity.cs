using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public class SpriteEntity : Entity
{
    protected Art _art;
    protected Texture2D _texture;

    public SpriteEntity(Vector2 position, Art art) : base(position)
    {
        _texture = AssetManager.GetTexture(art);
        _label = "sprite";
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Apply global transition offset for scene sliding (don't offset UI elements)
        Vector2 drawPos = _position;
        if (_label != "ui") drawPos += ScreenManager.TransitionOffset;
        spriteBatch.Draw(_texture, drawPos, Color.White);
    }
}

