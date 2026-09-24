using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WorldGenSandbox.Physics;

internal interface ITentacleRope
{
    public Vector2 AnchorStart { get; set; }
    public Vector2 AnchorEnd { get; set; }

    public void Update(int steps);
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel);
}
