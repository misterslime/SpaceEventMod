using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.GameObjects.FirmamentSea;

public partial class FirmamentSeaSystem : ModSystem
{
    public FirmamentSea UpdateSprings(FirmamentSea sea, float dampening, float tension)
    {
        FirmamentSea newSea = sea;

        for (int chunk = 0; chunk < newSea.Springs.Length; chunk++)
        {
            for (int spring = 0; spring < newSea.Springs[chunk].Length; spring++)
            {
                float acceleration = (-tension * newSea.Springs[chunk][spring].Position) - (dampening * newSea.Springs[chunk][spring].Velocity);

                // euler integration
                Spring newSpring = newSea.Springs[chunk][spring];
                newSpring.Velocity += acceleration;
                newSpring.Position += newSpring.Velocity;

                newSea.Springs[chunk][spring] = newSpring;
            }
        }

        return newSea;
    }

    public FirmamentSea PropagateWaves(FirmamentSea sea, float spread, int passes = 8)
    {
        FirmamentSea newSea = sea;

        float clampedSpread = MathHelper.Clamp(spread, 0f, 0.5f);

        float[,] leftDeltas = new float[sea.Springs.Length, sea.ChunkSize];
        float[,] rightDeltas = new float[sea.Springs.Length, sea.ChunkSize];

        // do some passes where springs pull on their neighbours
        for (int j = 0; j < passes; j++)
        {
            for (int chunk = 0; chunk < newSea.Springs.Length; chunk++)
            {
                for (int spring = 0; spring < newSea.Springs[chunk].Length; spring++)
                {
                    int index = chunk * spring;

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

            for (int chunk = 0; chunk < newSea.Springs.Length; chunk++)
            {
                for (int spring = 0; spring < newSea.Springs[chunk].Length; spring++)
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

    public FirmamentSea CollideSprings(FirmamentSea sea)
    {
        FirmamentSea newSea = sea;

        // sea surface collisions
        for (int chunk = 0; chunk < newSea.Springs.Length; chunk++)
        {
            for (int spring = 0; spring < newSea.Springs[chunk].Length; spring++)
            {
                var node = newSea.Springs[chunk][spring];
                var nodeLocation = chunk * sea.ChunkSize + spring;

                var nodePosition = sea.Position + new Vector2(sea.NodeWidth * nodeLocation, node.Position);

                foreach (var player in Main.ActivePlayers)
                {
                    if (player.getRect().Contains(new Point((int)nodePosition.X, (int)nodePosition.Y)))
                    {
                        node.Velocity = player.velocity.Y;
                    }
                }

                Spring? next = null;

                if (spring < sea.Springs[chunk].Length - 1)
                    next = newSea.Springs[chunk][spring + 1];
                else if (chunk < sea.Springs.Length - 1)
                    next = newSea.Springs[chunk + 1][0];

                if (next is not null)
                {
                    foreach (Projectile projectile in Main.ActiveProjectiles)
                    {
                        Vector2 end = sea.Position + new Vector2(sea.NodeWidth * (nodeLocation + 1), next.Value.Position);

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

    public bool LineRect(Vector2 lineStart, Vector2 lineEnd, Rectangle rectangle)
    {
        var left = LineLine(lineStart, lineEnd, rectangle.TopLeft(), rectangle.BottomLeft());
        var right = LineLine(lineStart, lineEnd, rectangle.TopRight(), rectangle.BottomRight());
        var top = LineLine(lineStart, lineEnd, rectangle.TopLeft(), rectangle.TopRight());
        var bottom = LineLine(lineStart, lineEnd, rectangle.BottomLeft(), rectangle.BottomRight());

        return left || right || top || bottom;
    }

    public bool LineLine(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
    {
        var uA = ((line2End.X - line2Start.X) * (line1Start.Y - line2Start.Y) - (line2End.Y - line2Start.Y) * (line1Start.X - line2Start.X)) / ((line2End.Y - line2Start.Y) * (line1End.X - line1Start.X) - (line2End.X - line2Start.X) * (line1End.Y - line1Start.Y));

        var uB = ((line1End.X - line1Start.X) * (line1Start.Y - line2Start.Y) - (line1End.Y - line1Start.Y) * (line1Start.X - line2Start.X)) / ((line2End.Y - line2Start.Y) * (line1End.X - line1Start.X) - (line2End.X - line2Start.X) * (line1End.Y - line1Start.Y));

        return uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1;

        //float intersectionX = line1Start.X + (uA * (line1End.X - line1Start.X));
        //float intersectionY = line1Start.Y + (uA * (line1End.Y - line1Start.Y));
    }
}
