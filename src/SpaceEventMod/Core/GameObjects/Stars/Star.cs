using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace SpaceEventMod.Core.GameObjects.Stars;

public struct Star(Vector2 spawnPosition)
{
    public Vector2 Position = spawnPosition;
    public string TexturePath = "SpaceEventMod/Assets/Textures/Props/Star";
    public int Width = 160;
    public int Height = 160;
    public float Rotation = 0;

    public int Durability = 1000;

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

