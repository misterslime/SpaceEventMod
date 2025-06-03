using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Physics;
using Terraria;

namespace SpaceEventMod.Core.GameObjects.Asteroids;

public struct Asteroid(Vector2Dynamics solver, Vector2 spawnPosition, string texturePath, int width, int height)
{
    public Vector2 Position = spawnPosition;
    public Vector2Dynamics SecondOrderSolver = solver;
    public string TexturePath = texturePath;
    public int Width = width;
    public int Height = height;

    public int Durability = 200;

    public Vector2 RestPosition = spawnPosition;
    public bool BeingStoodOn = false;

    public Vector2 ShakeDirection = Vector2.UnitX;
    public int ShakeTime = 0;

    public Vector2 SpriteDisplacement = Vector2.Zero;
    public SpriteEffects Effects = Main.rand.NextBool(2) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    public int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);

    public Rectangle GetBoundingBox()
    {
        return new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
    }

    public Vector2 GetCenter()
    {
        return this.Position + new Vector2(this.Width, this.Height) * 0.5f;
    }
}
