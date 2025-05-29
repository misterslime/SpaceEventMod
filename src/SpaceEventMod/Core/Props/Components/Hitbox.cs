using Microsoft.Xna.Framework;

namespace SpaceEventMod.Core.Props.Components;

public class Hitbox : Component
{
    public int Width;
    public int Height;
}

// this is here because im lazy :fire:
public static class HitboxUtils
{
    public static Rectangle GetBoundingBox(this Hitbox hitbox)
    {
        Vector2 position = hitbox.GetComponent<Transformation>().Position;

        return new Rectangle((int)position.X, (int)position.Y, hitbox.Width, hitbox.Height);
    }

    public static Vector2 GetCenter(this Hitbox hitbox)
    {
        Vector2 position = hitbox.GetComponent<Transformation>().Position;
        position += new Vector2(hitbox.Width, hitbox.Height) * 0.5f;

        return position;
    }
}
