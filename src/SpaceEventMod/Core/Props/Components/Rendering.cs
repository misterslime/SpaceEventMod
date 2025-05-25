using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Props.Systems;
using System.Drawing;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class Rendering : Component
{
    public delegate void RenderingDelegate();

    public RenderingDelegate OnRender { get; set; } = null;

    public Rendering()
    {
        RenderingSystem.Register(this);
    }

    public override void Dispose()
    {
        RenderingSystem.Unregister(this);
    }
}
