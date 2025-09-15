using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Core.Physics;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.Space.Rendering;

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

            BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
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

            if (BackgroundRenderTarget == null || SeaRenderTarget.Width != Main.screenWidth / 2 || SeaRenderTarget.Height != Main.screenHeight / 2)
            {
                BackgroundRenderTarget?.Dispose();
                BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            }

            Main.graphics.GraphicsDevice.SetRenderTarget(SeaRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            DrawBackgroundPrimitives(Color.Blue, Color.Magenta);
            DrawForegroundPrimitives();

            Main.graphics.GraphicsDevice.SetRenderTarget(BackgroundRenderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            SeaBackground.DrawBackground(DrawBackgroundPrimitives);

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

        }

        orig();
    }

    public void DrawBackgroundPrimitives(Color top, Color bottom)
    {
        if (Sea.Springs is null || !Sea.Active)
            return;

        SpaceEventMod.PrimitiveBatch.Begin(PrimitiveType.TriangleList);

        for (var chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
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
                    SpaceEventMod.PrimitiveBatch
                        .AddVertex(point1, bottom)
                        .AddVertex(point2, top)
                        .AddVertex(point3, top)
                        .AddVertex(point4, bottom)
                        .AddVertex(point1, bottom)
                        .AddVertex(point3, top);
                }
            }
        }

        SpaceEventMod.PrimitiveBatch.End();
    }

    private void DrawForegroundPrimitives()
    {
        if (Sea.Springs is null || !Sea.Active)
            return;

        SpaceEventMod.PrimitiveBatch.Begin(PrimitiveType.TriangleList);

        for (var chunk = 0; chunk < Sea.Springs.Length; chunk++)
        {
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
                    SpaceEventMod.PrimitiveBatch
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point2, Color.Transparent)
                        .AddVertex(point3, Color.Transparent)
                        .AddVertex(point4, new Color(0, 255, 0, 0))
                        .AddVertex(point1, new Color(0, 255, 0, 0))
                        .AddVertex(point3, Color.Transparent);
                }
            }
        }

        SpaceEventMod.PrimitiveBatch.End();
    }
}
