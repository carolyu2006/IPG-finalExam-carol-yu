using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public class Door : SpriteEntity
{
    protected string _nextScene;

    public string NextScene => _nextScene;

    public Door(Vector2 position, Art art, String nextScene) : base(position, art)
    {
        _label = "door";
        _nextScene = nextScene;
        _texture = AssetManager.GetTexture(art);
    }
}