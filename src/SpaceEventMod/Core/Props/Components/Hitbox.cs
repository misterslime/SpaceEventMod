using Microsoft.Xna.Framework;

namespace SpaceEventMod.Core.Props.Components;

/// <summary>
/// Makes this prop have physical dimensions.<br/>
/// Requires the <see cref="Transformation"/> component for certain functions involving this component to work, however its not needed for the component itself to work.
/// </summary>
/// <param name="width">Width of the prop.</param>
/// <param name="height">Height of the prop.</param>
public class Hitbox(int width, int height) : Component
{
    public int Width = width;
    public int Height = height;
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
