using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Content.Events.Space.LevelElements;

public struct SeaPosition(int left)
{
    public int Left = left;
    public Kinematics<float> Height = new Kinematics<float>(0);
}

public struct FirmamentSea
{
    public FirmamentSea(float nodeWidth, int chunkSize, int chunks)
    {
        NodeWidth = nodeWidth;
        ChunkSize = chunkSize;
        Chunks = chunks;

        var chunkWorldSize = (int)(nodeWidth * chunkSize);

        SeaPos = new SeaPosition((int)(Main.LocalPlayer.Center.X / chunkWorldSize) - chunks / 2);

        var springs = new Spring[chunks][];

        for (var i = 0; i < springs.Length; i++)
            springs[i] = new Spring[chunkSize];

        Springs = springs;

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

        SineOffsets = sineOffsets.ToArray();
        SineAmplitudes = sineAmplitudes.ToArray();
        SineStretches = sineStretches.ToArray();
        OffsetStretches = offsetStretches.ToArray();

        Active = true;
        Despawning = false;
    }

    public bool Despawning = true;
    public bool Active = false;
    public SeaPosition SeaPos;
    public float NodeWidth;
    public int ChunkSize;
    public int Chunks;

    public Spring[][] Springs;

    public float[] SineOffsets;
    public float[] SineAmplitudes;
    public float[] SineStretches;
    public float[] OffsetStretches;

    public bool CanSpawnThings { get => !Despawning && Math.Abs(SeaPos.Height.Velocity) < 1; }

    public Vector2 Position { get => new Vector2(SeaPos.Left * ChunkSize * NodeWidth, SeaPos.Height.Position); }

    public float OverlapSines(float x)
    {
        float result = 0;

        for (var i = 0; i < 7; i++)
            result += SineOffsets[i] + SineAmplitudes[i] * MathF.Sin(x * SineStretches[i] + Main.GlobalTimeWrappedHourly * OffsetStretches[i]);

        return result;
    }

    public FirmamentSea UpdateSeaHeight()
    {
        var newSea = this;

        var despawn = new FloatDynamics(1f / 500f, 0.5f, -0.5f);
        var spawn = new FloatDynamics(1f / 200f, 1f, 0.6f);

        if (Despawning)
            newSea.SeaPos.Height = despawn.Update(1f, SeaPos.Height, 0f);
        else
            newSea.SeaPos.Height = spawn.Update(1f, SeaPos.Height, (float)(Main.worldSurface * 0.35f * 16f));

        return newSea;
    }

    public FirmamentSea UpdateChunks()
    {
        var chunkWorldSize = (int)(NodeWidth * ChunkSize);
        var targetPosition = (int)Math.Floor(Main.LocalPlayer.Center.X / chunkWorldSize) - Springs.Length / 2;

        if (targetPosition == SeaPos.Left)
            return this;

        var seaPositionDelta = targetPosition - SeaPos.Left;

        var newSea = this;

        if (seaPositionDelta < 0)
            newSea.Springs = [new Spring[ChunkSize], Springs[0], Springs[1]];
        else if (seaPositionDelta > 0)
            newSea.Springs = [Springs[1], Springs[2], new Spring[ChunkSize]];

        newSea.SeaPos.Left = targetPosition;

        return newSea;
    }

    public FirmamentSea UpdateSprings(float dampening, float tension)
    {
        var newSea = this;

        for (var chunk = 0; chunk < newSea.Springs.Length; chunk++)
        {
            for (var spring = 0; spring < newSea.Springs[chunk].Length; spring++)
            {
                var acceleration = -tension * newSea.Springs[chunk][spring].Position - dampening * newSea.Springs[chunk][spring].Velocity;

                // euler integration
                var newSpring = newSea.Springs[chunk][spring];
                newSpring.Velocity += acceleration;
                newSpring.Position += newSpring.Velocity;

                newSea.Springs[chunk][spring] = newSpring;
            }
        }

        return newSea;
    }

    public FirmamentSea PropagateWaves(float spread, int passes = 8)
    {
        var newSea = this;

        var clampedSpread = MathHelper.Clamp(spread, 0f, 0.5f);

        var leftDeltas = new float[Springs.Length, ChunkSize];
        var rightDeltas = new float[Springs.Length, ChunkSize];

        // do some passes where springs pull on their neighbours
        for (var j = 0; j < passes; j++)
        {
            for (var chunk = 0; chunk < newSea.Springs.Length; chunk++)
            {
                for (var spring = 0; spring < newSea.Springs[chunk].Length; spring++)
                {
                    var index = chunk * spring;

                    if (spring > 0)
                    {
                        leftDeltas[chunk, spring] = clampedSpread * (newSea.Springs[chunk][spring].Position - newSea.Springs[chunk][spring - 1].Position);
                        newSea.Springs[chunk][spring - 1].Velocity += leftDeltas[chunk, spring];
                    }
                    else if (chunk > 0)
                    {
                        leftDeltas[chunk, spring] = clampedSpread * (newSea.Springs[chunk][spring].Position - newSea.Springs[chunk - 1][newSea.Springs[chunk].Length - 1].Position);
                        newSea.Springs[chunk - 1][newSea.Springs[chunk].Length - 1].Velocity += leftDeltas[chunk, spring];
                    }

                    if (spring < newSea.Springs[chunk].Length - 1)
                    {
                        rightDeltas[chunk, spring] = clampedSpread * (newSea.Springs[chunk][spring].Position - newSea.Springs[chunk][spring + 1].Position);
                        newSea.Springs[chunk][spring + 1].Velocity += rightDeltas[chunk, spring];
                    }
                    else if (chunk < newSea.Springs.Length - 1)
                    {
                        rightDeltas[chunk, spring] = clampedSpread * (newSea.Springs[chunk][spring].Position - newSea.Springs[chunk + 1][0].Position);
                        newSea.Springs[chunk + 1][0].Velocity += rightDeltas[chunk, spring];
                    }
                }
            }

            for (var chunk = 0; chunk < newSea.Springs.Length; chunk++)
            {
                for (var spring = 0; spring < newSea.Springs[chunk].Length; spring++)
                {
                    if (spring > 0)
                        newSea.Springs[chunk][spring - 1].Position += leftDeltas[chunk, spring];
                    else if (chunk > 0)
                        newSea.Springs[chunk - 1][newSea.Springs[chunk].Length - 1].Position += leftDeltas[chunk, spring];

                    if (spring < newSea.Springs[chunk].Length - 1)
                        newSea.Springs[chunk][spring + 1].Position += rightDeltas[chunk, spring];
                    else if (chunk < newSea.Springs.Length - 1)
                        newSea.Springs[chunk + 1][0].Position += rightDeltas[chunk, spring];
                }
            }
        }

        return newSea;
    }

    public FirmamentSea CollideSprings()
    {
        var newSea = this;

        // sea surface collisions
        for (var chunk = 0; chunk < newSea.Springs.Length; chunk++)
        {
            for (var spring = 0; spring < newSea.Springs[chunk].Length; spring++)
            {
                var node = newSea.Springs[chunk][spring];
                var nodeLocation = chunk * ChunkSize + spring;

                var nodePosition = Position + new Vector2(NodeWidth * nodeLocation, node.Position);

                foreach (var player in Main.ActivePlayers)
                {
                    if (player.getRect().Contains(new Point((int)nodePosition.X, (int)nodePosition.Y)))
                    {
                        node.Velocity = player.velocity.Y * 1.7f;
                    }
                }

                Spring? next = null;

                if (spring < Springs[chunk].Length - 1)
                    next = newSea.Springs[chunk][spring + 1];
                else if (chunk < Springs.Length - 1)
                    next = newSea.Springs[chunk + 1][0];

                if (next is not null)
                {
                    foreach (var projectile in Main.ActiveProjectiles)
                    {
                        var end = Position + new Vector2(NodeWidth * (nodeLocation + 1), next.Value.Position);

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
                    }
                }


                newSea.Springs[chunk][spring] = node;
            }
        }

        return newSea;
    }

    public FirmamentSea CheckIfShouldDeactivate()
    {
        var newSea = this;

        newSea.Active = Math.Abs(SeaPos.Height.Velocity) < 1 && Despawning;

        return newSea;
    }

    private bool LineRect(Vector2 lineStart, Vector2 lineEnd, Rectangle rectangle)
    {
        var left = LineLine(lineStart, lineEnd, rectangle.TopLeft(), rectangle.BottomLeft());
        var right = LineLine(lineStart, lineEnd, rectangle.TopRight(), rectangle.BottomRight());
        var top = LineLine(lineStart, lineEnd, rectangle.TopLeft(), rectangle.TopRight());
        var bottom = LineLine(lineStart, lineEnd, rectangle.BottomLeft(), rectangle.BottomRight());

        return left || right || top || bottom;
    }

    private bool LineLine(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
    {
        var uA = ((line2End.X - line2Start.X) * (line1Start.Y - line2Start.Y) - (line2End.Y - line2Start.Y) * (line1Start.X - line2Start.X)) / ((line2End.Y - line2Start.Y) * (line1End.X - line1Start.X) - (line2End.X - line2Start.X) * (line1End.Y - line1Start.Y));

        var uB = ((line1End.X - line1Start.X) * (line1Start.Y - line2Start.Y) - (line1End.Y - line1Start.Y) * (line1Start.X - line2Start.X)) / ((line2End.Y - line2Start.Y) * (line1End.X - line1Start.X) - (line2End.X - line2Start.X) * (line1End.Y - line1Start.Y));

        return uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1;

        //float intersectionX = line1Start.X + (uA * (line1End.X - line1Start.X));
        //float intersectionY = line1Start.Y + (uA * (line1End.Y - line1Start.Y));
    }
}