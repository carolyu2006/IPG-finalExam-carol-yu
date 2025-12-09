using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

public class AnimationEntity : Entity
{
    private Texture2D[] _frames;
    private float _frameTime = 0.12f;
    private float _timer = 0f;
    private int _currentFrame = 0;
    private bool _loop = true;

    // Optional callback invoked when a non-looping animation finishes
    public System.Action OnFinished;

    public AnimationEntity(Vector2 position, Texture2D[] frames, float frameTime = 0.12f, bool loop = true) : base(position)
    {
        _frames = frames ?? new Texture2D[0];
        _frameTime = frameTime;
        _loop = loop;
        _label = "animation";
    }

    public override void Update()
    {
        base.Update();
        if (_frames.Length == 0) return;

        _timer += ServiceLocator.DeltaSeconds;
        if (_timer > _frameTime)
        {
            _timer -= _frameTime;
            _currentFrame++;
            if (_currentFrame >= _frames.Length)
            {
                if (_loop)
                {
                    _currentFrame = 0;
                }
                else
                {
                    try
                    {
                        System.Action cb = OnFinished;
                        if (cb != null) cb();
                    }
                    catch { }
                    Die();
                    return;
                }
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_frames.Length == 0) return;
        Texture2D tex = _frames[_currentFrame];
        Vector2 drawPos = _position;
        if (_label != "ui") drawPos += ScreenManager.TransitionOffset;
        spriteBatch.Draw(tex, drawPos, Color.White);
    }
}