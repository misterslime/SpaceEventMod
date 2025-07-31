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

    public static void CreateSea(Vector2 position)
    {
        var sea = new FirmamentSea();

        sea.Position = position;
        sea.NodeWidth = 16;
        sea.Active = true;

        var count = 64;

        var nodes = new HookeSpring[count];

        for (var i = 0; i < nodes.Length; i++)
        {
            var node = new HookeSpring();

            node.Height = 0;

            nodes[i] = node;
        }

        sea.Nodes = nodes;
        sea.Spread = 0.1f;

        var sineOffsets = new List<float>();
        var sineAmplitudes = new List<float>();
        var sineStretches = new List<float>();
        var offsetStretches = new List<float>();

        for (var i = 0; i < 7; i++)
        {
            sineOffsets.Add(-1 + 2 * Main.rand.NextFloat());
            sineAmplitudes.Add(5f * Main.rand.NextFloat());
            sineStretches.Add(0.05f * Main.rand.NextFloat());
            offsetStretches.Add(10f * Main.rand.NextFloat());
        }

        sea.SineOffsets = sineOffsets.ToArray();
        sea.SineAmplitudes = sineAmplitudes.ToArray();
        sea.SineStretches = sineStretches.ToArray();
        sea.OffsetStretches = offsetStretches.ToArray();

        firmamentSea = sea;
    }
}
