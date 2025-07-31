using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Core.GameObjects.FirmamentSea;

// https://code.tutsplus.com/make-a-splash-with-dynamic-2d-water-effects--gamedev-236t
// made with this thingy
// to-do: make it generate infinitely with chunks
public partial class FirmamentSeaSystem : ModSystem
{
    public static FirmamentSea firmamentSea;

    public override void Load()
    {
        firmamentSea = new FirmamentSea();

        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths += DrawToTarget;
            On_Main.DrawDust += DrawSea;

            BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        });
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths -= DrawToTarget;
            On_Main.DrawDust -= DrawSea;

            BackgroundRenderTarget?.Dispose();
            BackgroundRenderTarget = null;
        });
    }
}
