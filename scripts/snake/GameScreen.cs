using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

// Base class for all screens managed by ScreenManager
public abstract class GameScreen
{
    // Called when the screen becomes current
    public virtual void Load() { }
    // Called when the screen is no longer current
    public virtual void Unload() { }
    // Called every frame
    public virtual void Update(GameTime gameTime) { }
    // Called every frame after Update
    public virtual void Draw(SpriteBatch spriteBatch) { }
}
