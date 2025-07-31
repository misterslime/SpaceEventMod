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
        if (firmamentSea.Nodes is null)
            return;

        const float k = 0.012f; // adjust this value to your liking

        var sea = firmamentSea;

        var nodes = firmamentSea.Nodes;

        nodes = nodes.UpdateArray(0.05f, 0.025f).PropagateWaves(0.25f, 8);

        // sea surface collisions
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];

            var nodePosition = sea.Position + new Vector2(sea.NodeWidth * i, node.Height);

            foreach (var player in Main.ActivePlayers)
            {
                if (player.getRect().Contains(new Point((int)nodePosition.X, (int)nodePosition.Y)))
                {
                    node.Velocity = player.velocity.Y * 2f;
                }
            }

            foreach (var projectile in Main.ActiveProjectiles)
            {
                if (i < nodes.Length - 1)
                {
                    var nodeNext = nodes[i + 1];

                    var end = sea.Position + new Vector2(sea.NodeWidth * (i + 1), nodeNext.Height);

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

    public void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        var r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        var v = Vector2.Normalize(begin - end);
        var angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(SpaceEventMod.WhitePixel, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}
