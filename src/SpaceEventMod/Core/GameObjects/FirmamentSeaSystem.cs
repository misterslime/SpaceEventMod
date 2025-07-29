using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Core.Geometry;
using SpaceEventMod.Core.Graphics;
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

namespace SpaceEventMod.Core.GameObjects;

public struct SeaSurfaceNode()
{
    public float Height, Velocity = 0;
}

public struct FirmamentSea
{
    public Vector2 Position;
    public float NodeWidth;
    public bool Active;
    public float Spread;

    public SeaSurfaceNode[] Nodes;

    public float[] SineOffsets;
    public float[] SineAmplitudes;
    public float[] SineStretches;
    public float[] OffsetStretches;

    public float OverlapSines(float x)
    {
        float result = 0;

        for (int i = 0; i < 7; i++)
        {
            result += SineOffsets[i] + SineAmplitudes[i] * MathF.Sin(x * SineStretches[i] + Main.GlobalTimeWrappedHourly * OffsetStretches[i]);
        }

        return result;
    }
}

// https://code.tutsplus.com/make-a-splash-with-dynamic-2d-water-effects--gamedev-236t
// made with this thingy
public class FirmamentSeaSystem : ModSystem
{
    public static FirmamentSea firmamentSea;
    public static RenderTarget2D BackgroundRenderTarget;

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

    private static void DrawToTarget(On_Main.orig_CheckMonoliths orig)
    {
        if (Main.gameMenu)
        {
            orig();
            return;
        }

        if (BackgroundRenderTarget == null || BackgroundRenderTarget.Width != Main.screenWidth || BackgroundRenderTarget.Height != Main.screenHeight)
        {
            BackgroundRenderTarget?.Dispose();
            BackgroundRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }

        Main.graphics.GraphicsDevice.SetRenderTarget(BackgroundRenderTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Black);

        if (firmamentSea.Nodes is not null)
        {
            float GetNormalHeight(float height)
            {
                if (height > 0 && height < 1)
                    return 1;
                else if (height <= 0 && height > -1)
                    return 1;

                return MathF.Abs(height);
            }

            Color midnightBlue = new Color(10, 0, 100);
            Color lightBlue = new Color(0, 78, 255);

            SpaceEventMod.PrimitiveBatch.Begin(PrimitiveType.TriangleList);

            for (int i = 0; i < firmamentSea.Nodes.Length - 1; i++)
            {
                float difference = firmamentSea.Nodes[i + 1].Height - firmamentSea.Nodes[i].Height;

                float waveOffset = firmamentSea.OverlapSines((float)(firmamentSea.NodeWidth * i)) / GetNormalHeight(difference * MathF.PI);
                float waveOffset2 = firmamentSea.OverlapSines((float)(firmamentSea.NodeWidth * (i + 1))) / GetNormalHeight(difference * MathF.PI);

                Vector2 begin = firmamentSea.Position + new Vector2(firmamentSea.NodeWidth * i, firmamentSea.Nodes[i].Height + waveOffset) - Main.screenPosition;
                Vector2 end = firmamentSea.Position + new Vector2(firmamentSea.NodeWidth * (i + 1), firmamentSea.Nodes[i + 1].Height + waveOffset2) - Main.screenPosition;

                begin *= 0.5f;
                begin = new Vector2(MathF.Floor(begin.X), MathF.Floor(begin.Y));
                begin *= 2f;

                end *= 0.5f;
                end = new Vector2(MathF.Floor(end.X), MathF.Floor(end.Y));
                end *= 2f;

                Vector2 point1 = begin;
                Vector2 point2 = new Vector2(begin.X, begin.Y - 240f);
                Vector2 point3 = new Vector2(end.X, end.Y - 240f);
                Vector2 point4 = end;

                SpaceEventMod.PrimitiveBatch
                    .AddVertex(point1, Color.White)
                    .AddVertex(point2, Color.Black)
                    .AddVertex(point3, Color.Black)
                    .AddVertex(point4, Color.White)
                    .AddVertex(point1, Color.White)
                    .AddVertex(point3, Color.Black);
            }

            SpaceEventMod.PrimitiveBatch.End();
        }

        Main.instance.GraphicsDevice.SetRenderTarget(null);

        orig();
    }

    public static void CreateSea(Vector2 position)
    {
        FirmamentSea sea = new FirmamentSea();

        sea.Position = position;
        sea.NodeWidth = 16;
        sea.Active = true;

        int count = (int)(Main.screenWidth * 1.5f / sea.NodeWidth);

        SeaSurfaceNode[] nodes = new SeaSurfaceNode[count];

        for (int i = 0; i < nodes.Length; i++)
        {
            SeaSurfaceNode node = new SeaSurfaceNode();

            node.Height = 0;

            nodes[i] = node;
        }

        sea.Nodes = nodes;
        sea.Spread = 0.1f;

        List<float> sineOffsets = new List<float>();
        List<float> sineAmplitudes = new List<float>();
        List<float> sineStretches = new List<float>();
        List<float> offsetStretches = new List<float>();

        for (int i = 0; i < 7; i++)
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

    public override void PostUpdatePlayers()
    {
        if (firmamentSea.Nodes is null)
            return;

        const float k = 0.012f; // adjust this value to your liking

        FirmamentSea sea = firmamentSea;

        SeaSurfaceNode[] nodes = firmamentSea.Nodes;

        for (int i = 0; i < nodes.Length; i++)
        {
            SeaSurfaceNode node = nodes[i];

            float x = node.Height;
            float acceleration = -k * x - node.Velocity * 0.3f;

            node.Height += node.Velocity;
            node.Velocity += acceleration;

            nodes[i] = node;
        }

        float[] leftDeltas = new float[nodes.Length];
        float[] rightDeltas = new float[nodes.Length];

        // do some passes where springs pull on their neighbours
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (i > 0)
                {
                    SeaSurfaceNode leftNode = nodes[i - 1];

                    leftDeltas[i] = sea.Spread * (nodes[i].Height - nodes[i - 1].Height);
                    leftNode.Velocity += leftDeltas[i];

                    nodes[i - 1] = leftNode;
                }

                if (i < nodes.Length - 1)
                {
                    SeaSurfaceNode rightNode = nodes[i + 1];

                    rightDeltas[i] = sea.Spread * (nodes[i].Height - nodes[i + 1].Height);
                    rightNode.Velocity += rightDeltas[i];

                    nodes[i + 1] = rightNode;
                }
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                if (i > 0)
                {
                    SeaSurfaceNode leftNode = nodes[i - 1];

                    leftNode.Height += leftDeltas[i];

                    nodes[i - 1] = leftNode;
                }
                if (i < nodes.Length - 1)
                {
                    SeaSurfaceNode rightNode = nodes[i + 1];

                    rightNode.Height += rightDeltas[i];

                    nodes[i + 1] = rightNode;
                }
            }
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            SeaSurfaceNode node = nodes[i];

            Vector2 nodePosition = sea.Position + new Vector2(sea.NodeWidth * i, node.Height);

            foreach (Player player in Main.ActivePlayers)
            {
                if (player.getRect().Contains(new Point((int)nodePosition.X, (int)nodePosition.Y)))
                {
                    node.Velocity = player.velocity.Y * 2f;
                }
            }

            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (i < nodes.Length - 1)
                {
                    SeaSurfaceNode nodeNext = nodes[i + 1];

                    Vector2 end = sea.Position + new Vector2(sea.NodeWidth * (i + 1), nodeNext.Height);

                    if (!(projectile.getRect().Left > end.X || projectile.getRect().Right < nodePosition.X))
                    {
                        if (LineLine(nodePosition, end, projectile.Center - projectile.velocity * 3f, projectile.Center + projectile.velocity))
                        {
                            node.Velocity = projectile.velocity.Y;
                            projectile.Kill();
                        }

                        if (LineRect(nodePosition, end, projectile.getRect()))
                        {
                            node.Velocity = projectile.velocity.Y;
                            projectile.Kill();
                        }
                    }

                    continue;
                }
            }

            nodes[i] = node;
        }

        sea.Nodes = nodes;

        firmamentSea = sea;
    }

    public void DrawSea(On_Main.orig_DrawDust orig, Main self)
    {
        if (BackgroundRenderTarget == null || firmamentSea.Nodes is null)
            return;

        float GetNormalHeight(float height)
        {
            if (height > 0 && height < 1)
                return 1;
            else if (height <= 0 && height > -1)
                return 1;

            return MathF.Abs(height);
        }

        PixelRenderer.Draw(null, PrimitiveType.TriangleList, (PrimitiveBatch primitiveBatch) =>
        {
            Color midnightBlue = new Color(10, 0, 100);
            Color lightBlue = new Color(0, 78, 255);

            for (int i = 0; i < firmamentSea.Nodes.Length - 1; i++)
            {
                float difference = firmamentSea.Nodes[i + 1].Height - firmamentSea.Nodes[i].Height;

                float waveOffset = firmamentSea.OverlapSines((float)(firmamentSea.NodeWidth * i)) / GetNormalHeight(difference * MathF.PI);
                float waveOffset2 = firmamentSea.OverlapSines((float)(firmamentSea.NodeWidth * (i + 1))) / GetNormalHeight(difference * MathF.PI);

                Vector2 begin = firmamentSea.Position + new Vector2(firmamentSea.NodeWidth * i, firmamentSea.Nodes[i].Height + waveOffset) - Main.screenPosition;
                Vector2 end = firmamentSea.Position + new Vector2(firmamentSea.NodeWidth * (i + 1), firmamentSea.Nodes[i + 1].Height + waveOffset2) - Main.screenPosition;

                begin *= 0.5f;
                begin = new Vector2(MathF.Floor(begin.X), MathF.Floor(begin.Y));
                begin *= 2f;

                end *= 0.5f;
                end = new Vector2(MathF.Floor(end.X), MathF.Floor(end.Y));
                end *= 2f;

                Vector2 point1 = begin;
                Vector2 point2 = new Vector2(begin.X, 0f);
                Vector2 point3 = new Vector2(end.X, 0f);
                Vector2 point4 = end;

                primitiveBatch
                    .AddVertex(point1, Color.Transparent)
                    .AddVertex(point2, midnightBlue)
                    .AddVertex(point3, midnightBlue)
                    .AddVertex(point4, Color.Transparent)
                    .AddVertex(point1, Color.Transparent)
                    .AddVertex(point3, midnightBlue);
            }
        });

        var inkStencilShader = Assets.Assets.Shaders.FirmamentSeaBackground.Value;

        inkStencilShader.Parameters["noise"].SetValue(Assets.Assets.Textures.Extra.Noise.Foam.Value);
        inkStencilShader.Parameters["palette"].SetValue(Assets.Assets.Textures.Extra.FirmamentSeaColors.Value);
        inkStencilShader.Parameters["globalTime"].SetValue(Main.GlobalTimeWrappedHourly);
        inkStencilShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        inkStencilShader.Parameters["screenWorldPosition"].SetValue(Main.screenPosition * 0.5f); // this is being halved because its being pixelated

        PixelRenderer.Draw(inkStencilShader, (SpriteBatch spriteBatch) =>
        {
            spriteBatch.Draw(BackgroundRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        });

        orig(self);
    }

    public bool LineRect(Vector2 lineStart, Vector2 lineEnd, Rectangle rectangle)
    {
        bool left = LineLine(lineStart, lineEnd, rectangle.TopLeft(), rectangle.BottomLeft());
        bool right = LineLine(lineStart, lineEnd, rectangle.TopRight(), rectangle.BottomRight());
        bool top = LineLine(lineStart, lineEnd, rectangle.TopLeft(), rectangle.TopRight());
        bool bottom = LineLine(lineStart, lineEnd, rectangle.BottomLeft(), rectangle.BottomRight());

        return left || right || top || bottom;
    }

    public bool LineLine(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
    {
        float uA = ((line2End.X - line2Start.X) * (line1Start.Y - line2Start.Y) - (line2End.Y - line2Start.Y) * (line1Start.X - line2Start.X)) / ((line2End.Y - line2Start.Y) * (line1End.X - line1Start.X) - (line2End.X - line2Start.X) * (line1End.Y - line1Start.Y));

        float uB = ((line1End.X - line1Start.X) * (line1Start.Y - line2Start.Y) - (line1End.Y - line1Start.Y) * (line1Start.X - line2Start.X)) / ((line2End.Y - line2Start.Y) * (line1End.X - line1Start.X) - (line2End.X - line2Start.X) * (line1End.Y - line1Start.Y));

        return uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1;

        //float intersectionX = line1Start.X + (uA * (line1End.X - line1Start.X));
        //float intersectionY = line1Start.Y + (uA * (line1End.Y - line1Start.Y));
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        Vector2 v = Vector2.Normalize(begin - end);
        float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(SpaceEventMod.WhitePixel, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}
