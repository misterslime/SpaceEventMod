using Microsoft.Xna.Framework;

namespace SpaceEventMod.Core.Props.Components;

public class Hitbox : Component
{
    public int Width;
    public int Height;

    public Rectangle GetBoundingBox()
    {
        Vector2 position = prop.GetComponent<Transformation>().Position;

        return new Rectangle((int)position.X, (int)position.Y, Width, Height);
    }

    public Vector2 GetCenter()
    {
        Vector2 position = prop.GetComponent<Transformation>().Position;
        position += new Vector2(Width, Height) * 0.5f;

        return position;
    }
}
