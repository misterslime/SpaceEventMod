using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Physics;
using Terraria;

namespace SpaceEventMod.Content.Space.LevelElements;

public struct Asteroid(Vector2 initialPosition, int variant, int width, int height)
{
    public PhysicsPoint Transform = new PhysicsPoint(initialPosition);
    public int Variant = variant;
    public int Width = width;
    public int Height = height;

    public int Durability = 200;

    public Vector2 RestPosition = initialPosition;
    public bool BeingStoodOn = false;

    public Vector2 ShakeDirection = Vector2.UnitX;
    public int ShakeTime = 0;

    public Vector2 SpriteDisplacement = Vector2.Zero;
    public SpriteEffects Effects = Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    public int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);

    public Rectangle GetBoundingBox()
    {
        var worldCoords = SpaceEvent.SeaToWorldCoordinates(Transform.Position);

        return new Rectangle((int)worldCoords.X + (int)SpriteDisplacement.Y, (int)worldCoords.Y + (int)SpriteDisplacement.Y, Width, Height);
    }

    public Vector2 GetCenter()
    {
        return SpaceEvent.SeaToWorldCoordinates(GetTrueCenter());
    }

    public Vector2 GetTrueCenter()
    {
        return Transform.Position + new Vector2(Width, Height) * 0.5f;
    }
}
