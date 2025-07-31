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
    public override void PostUpdatePlayers()
    {
        if (firmamentSea.Springs is null)
            return;

        var sea = firmamentSea;

        // update springs
        var springs = firmamentSea.Springs;

        springs = UpdateSprings(sea.Springs, 0.05f, 0.025f);
        springs = PropagateWaves(sea.Springs, 0.25f);
        springs = CollideSprings(sea.Springs, sea);

        firmamentSea = sea;
    }

    public HookeSpring[,] UpdateSprings(HookeSpring[,] springs, float dampening, float tension)
    {
        HookeSpring[,] newArray = springs;

        for (int chunk = 0; chunk < springs.GetLength(0); chunk++)
        {
            for (int spring = 0; spring < springs.GetLength(1); spring++)
            {
                float acceleration = (-tension * springs[chunk, spring].Height) - (dampening * springs[chunk, spring].Velocity);

                // euler integration
                HookeSpring newSpring = springs[chunk, spring];
                newSpring.Height += springs[chunk, spring].Velocity;
                newSpring.Velocity += acceleration;

                springs[chunk, spring] = newSpring;
            }
        }

        return newArray;
    }

    public HookeSpring[,] PropagateWaves(HookeSpring[,] springs, float spread, int passes = 8)
    {
        HookeSpring[,] newArray = springs;

        float clampedSpread = MathHelper.Clamp(spread, 0f, 0.5f);

        float[,] leftDeltas = new float[springs.GetLength(0), springs.GetLength(1)];
        float[,] rightDeltas = new float[springs.GetLength(0), springs.GetLength(1)];

        // do some passes where springs pull on their neighbours
        for (int j = 0; j < passes; j++)
        {
            for (int chunk = 0; chunk < springs.GetLength(0); chunk++)
            {
                for (int spring = 0; spring < springs.GetLength(1); spring++)
                {
                    int index = chunk * spring;

                    if (spring > 0)
                    {
                        leftDeltas[chunk, spring] = clampedSpread * (springs[chunk, spring].Height - springs[chunk, spring - 1].Height);
                        newArray[chunk, spring - 1].Velocity += leftDeltas[chunk, spring];
                    }
                    else if (chunk > 0)
                    {
                        leftDeltas[chunk, spring] = clampedSpread * (springs[chunk, spring].Height - springs[chunk - 1, springs.GetLength(1) - 1].Height);
                        newArray[chunk - 1, springs.GetLength(1) - 1].Velocity += leftDeltas[chunk, spring];
                    }

                    if (spring < springs.GetLength(1) - 1)
                    {
                        rightDeltas[chunk, spring] = clampedSpread * (springs[chunk, spring].Height - springs[chunk, spring + 1].Height);
                        newArray[chunk, spring + 1].Velocity += rightDeltas[chunk, spring];
                    }
                    else if (chunk < springs.GetLength(0) - 1)
                    {
                        rightDeltas[chunk, spring] = clampedSpread * (springs[chunk, spring].Height - springs[chunk + 1, 0].Height);
                        newArray[chunk + 1, 0].Velocity += rightDeltas[chunk, spring];
                    }
                }
            }

            for (int chunk = 0; chunk < springs.GetLength(0); chunk++)
            {
                for (int spring = 0; spring < springs.GetLength(1); spring++)
                {
                    if (spring > 0)
                        newArray[chunk, spring - 1].Height += leftDeltas[chunk, spring];
                    else if (chunk > 0)
                        newArray[chunk - 1, springs.GetLength(1) - 1].Height += leftDeltas[chunk, spring];

                    if (spring < springs.GetLength(1) - 1)
                        newArray[chunk, spring + 1].Height += rightDeltas[chunk, spring];
                    else if (chunk < springs.GetLength(0) - 1)
                        newArray[chunk + 1, 0].Height += rightDeltas[chunk, spring];
                }
            }
        }

        return newArray;
    }

    public HookeSpring[,] CollideSprings(HookeSpring[,] springs, FirmamentSea sea)
    {
        HookeSpring[,] newArray = springs;

        // sea surface collisions
        for (int chunk = 0; chunk < springs.GetLength(0); chunk++)
        {
            for (int spring = 0; spring < springs.GetLength(1); spring++)
            {
                var node = springs[chunk, spring];
                var nodeLocation = chunk * sea.ChunkSize + spring;

                var nodePosition = sea.Position + new Vector2(sea.NodeWidth * nodeLocation, node.Height);

                foreach (var player in Main.ActivePlayers)
                {
                    if (player.getRect().Contains(new Point((int)nodePosition.X, (int)nodePosition.Y)))
                    {
                        node.Velocity = player.velocity.Y * 2f;
                    }
                }

                HookeSpring? next = null;

                if (spring < springs.GetLength(1) - 1)
                    next = springs[chunk, spring + 1];
                else if (chunk < springs.GetLength(0) - 1)
                    next = springs[chunk + 1, 0];

                if (next is not null)
                {
                    foreach (Projectile projectile in Main.ActiveProjectiles)
                    {
                        Vector2 end = sea.Position + new Vector2(sea.NodeWidth * (nodeLocation + 1), next.Value.Height);

                        if (!(projectile.getRect().Left > end.X || projectile.getRect().Right < nodePosition.X))
                        {
                            if (LineLine(nodePosition, end, projectile.Center - projectile.velocity * 3f, projectile.Center + projectile.velocity))
                            {
                                node.Velocity = projectile.velocity.Y * 2f;
                                projectile.Kill();
                            }

                            if (LineRect(nodePosition, end, projectile.getRect()))
                            {
                                node.Velocity = projectile.velocity.Y * 2f;
                                projectile.Kill();
                            }
                        }
                    }
                }


                newArray[chunk, spring] = node;
            }
        }

        return newArray;
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
