using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
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
    public static FirmamentSea Sea;

    public override void Load()
    {
        Sea = new FirmamentSea();

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

    public override void PostUpdatePlayers()
    {
        if (Sea.Springs is null)
            return;

        var sea = UpdateChunks(Sea);

        // update springs
        sea = CollideSprings(sea);
        sea = PropagateWaves(sea, 0.025f);
        sea = UpdateSprings(sea, 0.05f, 0.01f);

        Sea = sea;
    }

    public FirmamentSea UpdateChunks(FirmamentSea sea)
    {
        int chunkWorldSize = (int)(sea.NodeWidth * sea.ChunkSize);
        int targetPosition = (int)Math.Floor(Main.LocalPlayer.Center.X / chunkWorldSize) - (sea.Springs.Length / 2);

        if (targetPosition == sea.SeaPos.Left)
            return sea;

        int seaPositionDelta = targetPosition - sea.SeaPos.Left;

        FirmamentSea newSea = sea;

        if (seaPositionDelta < 0)
            newSea.Springs = [new Spring[sea.ChunkSize], sea.Springs[0], sea.Springs[1], sea.Springs[2], sea.Springs[3]];
        else if (seaPositionDelta > 0)
            newSea.Springs = [sea.Springs[1], sea.Springs[2], sea.Springs[3], sea.Springs[4], new Spring[sea.ChunkSize]];

        newSea.SeaPos.Left = targetPosition;

        return newSea;
    }
}
