using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Template;

/// <summary>
/// Stub class for item pickups - customize for your game
/// </summary>
public class ItemPickUp : Entity
{
    public Item Item { get; set; }

    // Support multiple constructor signatures for compatibility
    public ItemPickUp(Vector2 position, Item item) : base(position)
    {
        Item = item;
        _rect = new Rectangle((int)position.X, (int)position.Y, 32, 32);
    }

    public ItemPickUp(Vector2 position, Art art, Item item) : base(position)
    {
        Item = item;
        _rect = new Rectangle((int)position.X, (int)position.Y, 32, 32);
    }

    public void Die()
    {
        // Mark as dead - implement removal logic
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Customize drawing logic here
    }
}
