using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.DataStructures;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Content.Space.LevelElements;
using SpaceEventMod.Core.Physics;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Rendering;

[Autoload(Side = ModSide.Client)]
public class SeaTargets : ILoadable
{
    public static RenderTarget2D SeaRenderTarget;

    public static RenderTarget2D BackgroundRenderTarget;

    private FirmamentSea Sea { get => SpaceEvent.Sea; }

    public void Load(Mod mod)
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths += DrawToTarget;

            SeaRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

            BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        });
    }

    public void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths -= DrawToTarget;

            SeaRenderTarget?.Dispose();
            SeaRenderTarget = null;

            BackgroundRenderTarget?.Dispose();
            BackgroundRenderTarget = null;
        });
    }

    private void DrawToTarget(On_Main.orig_CheckMonoliths orig)
    {
        if (!Main.gameMenu)
        {
            if (SeaRenderTarget == null || SeaRenderTarget.Width != Main.screenWidth || SeaRenderTarget.Height != Main.screenHeight)
            {
                SeaRenderTarget?.Dispose();
                SeaRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }

            if (BackgroundRenderTarget == null || SeaRenderTarget.Width != Main.screenWidth || SeaRenderTarget.Height != Main.screenHeight)
            {
                BackgroundRenderTarget?.Dispose();
                BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }

            Main.graphics.GraphicsDevice.SetRenderTarget(SeaRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Pipeline pipeline = Graphics.BeginPipeline();

            DrawBackgroundPrimitives(in pipeline, Color.Blue, Color.Magenta);
            DrawForegroundPrimitives(in pipeline);

            pipeline.Flush();

            Main.graphics.GraphicsDevice.SetRenderTarget(BackgroundRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            pipeline = Graphics.BeginPipeline();

            DrawBackgroundPrimitives(in pipeline, new Color(10, 0, 100), new Color(10, 0, 100));

            pipeline.Flush();

            SeaBackground.DrawBackground();

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

        }

        orig();
    }

    public void DrawBackgroundPrimitives(in Pipeline pipeline, Color top, Color bottom)
    {
        if (Sea.Springs is null || !Sea.Active)
            return;

        for (var chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
            Mesh seaChunk = new Mesh(PrimitiveType.TriangleList);

            for (var spring = 0; spring < Sea.Springs[chunk].Length; spring++)
            {
                Spring? next = null;
                var nodeLocation = chunk * Sea.ChunkSize + spring;

                if (spring < Sea.Springs[chunk].Length - 1)
                    next = Sea.Springs[chunk][spring + 1];
                else if (chunk < Sea.Springs.Length - 1)
                    next = Sea.Springs[chunk + 1][0];

                if (next is not null)
                {
                    var waveOffset = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * nodeLocation));
                    var waveOffset2 = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * (nodeLocation + 1)));

                    var seaScreenPosition = Sea.Position - Main.screenPosition;

                    var begin = new Vector2(Sea.NodeWidth * nodeLocation, Sea.Springs[chunk][spring].Position + waveOffset) + seaScreenPosition;
                    var end = new Vector2(Sea.NodeWidth * (nodeLocation + 1), next.Value.Position + waveOffset2) + seaScreenPosition;

                    var point1 = begin;
                    var point2 = new Vector2(begin.X, 0f);
                    var point3 = new Vector2(end.X, 0f);
                    var point4 = end;

                    var point5 = new Vector2(begin.X, begin.Y + 32f) * 0.5f;
                    var point6 = new Vector2(end.X, end.Y + 32f) * 0.5f;

                    // red is used to show how high up in the sea the pixel is
                    // blue to tell if the pixel is in the sea at all
                    seaChunk
                        .AddVertex(point1, bottom)
                        .AddVertex(point2, top)
                        .AddVertex(point3, top)
                        .AddVertex(point4, bottom)
                        .AddVertex(point1, bottom)
                        .AddVertex(point3, top);
                }
            }

            pipeline.DrawMesh(seaChunk, SpaceEventMod.basicEffect);
        }
    }

    private void DrawForegroundPrimitives(in Pipeline pipeline)
    {
        if (Sea.Springs is null || !Sea.Active)
            return;

        for (var chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
            Mesh seaChunk = new Mesh(PrimitiveType.TriangleList);

            for (var spring = 0; spring < Sea.Springs[chunk].Length; spring++)
            {
                Spring? next = null;
                var nodeLocation = chunk * Sea.ChunkSize + spring;

                if (spring < Sea.Springs[chunk].Length - 1)
                    next = Sea.Springs[chunk][spring + 1];
                else if (chunk < Sea.Springs.Length - 1)
                    next = Sea.Springs[chunk + 1][0];

                if (next is not null)
                {
                    var waveOffset = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * nodeLocation));
                    var waveOffset2 = Sea.OverlapSines((float)(Sea.Position.X + Sea.NodeWidth * (nodeLocation + 1)));

                    var seaScreenPosition = Sea.Position - Main.screenPosition;

                    var begin = new Vector2(Sea.NodeWidth * nodeLocation, Sea.Springs[chunk][spring].Position + waveOffset) + seaScreenPosition;
                    var end = new Vector2(Sea.NodeWidth * (nodeLocation + 1), next.Value.Position + waveOffset2) + seaScreenPosition;

                    var point1 = begin;
                    var point2 = new Vector2(begin.X, begin.Y - 120);
                    var point3 = new Vector2(end.X, end.Y - 120);
                    var point4 = end;

                    // red is used to show how high up in the sea the pixel is
                    // blue to tell if the pixel is in the sea at all
                    seaChunk
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point2, Color.Transparent)
                        .AddVertex(point3, Color.Transparent)
                        .AddVertex(point4, new Color(0, 255, 0, 0))
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point3, Color.Transparent);
                }
            }

            pipeline.DrawMesh(seaChunk, SpaceEventMod.basicEffect);
        }
    }
}
