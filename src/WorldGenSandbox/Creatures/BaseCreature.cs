using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldGenSandbox.Creatures;

internal abstract class BaseCreature
{
    public bool Active { get; set; } = false;

    public Vector2 Center { get; set; } = Vector2.Zero;

    public Vector2 Velocity {  get; set; } = Vector2.Zero;

    public virtual Color Color => Color.White;

    public abstract void AI();

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, Center / 16, Color);
    }
}
