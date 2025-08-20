using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Content.Events.Space.LevelElements;

public struct Asteroid(Vector2 initialPosition, float spawnHeight, int variant, int width, int height)
{
    public Kinematics<Vector2> Transform = new Kinematics<Vector2>(new Vector2(initialPosition.X, spawnHeight));
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
