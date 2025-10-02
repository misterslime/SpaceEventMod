using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Core.Physics.Collision;

internal static class TileCollisionHelper
{
    public static PhysicsPoint CheckPoint(PhysicsPoint point, int sampleSize, float pointRadius)
    {
        PhysicsPoint newPoint = point;

        Vector2 topLeft = point.Position - 8 * new Vector2(sampleSize);
        Point topLeftTiles = topLeft.ToTileCoordinates();

        for (int i = 0; i < sampleSize; i++)
        {
            for (int j = 0; j < sampleSize; j++)
            {
                Tile tile = Framing.GetTileSafely(topLeftTiles.X + i, topLeftTiles.Y + j);
                if (!tile.active() || !Main.tileSolid[tile.type])
                    continue;

                Vector2 tilePosition = new Vector2(topLeftTiles.X + i, topLeftTiles.Y + j) * 16;

                Rectangle tileRectangle = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, 16, 16);

                if (!tileRectangle.Contains(point.Position.ToPoint()))
                    continue;

                Circle circle = new Circle(point.Position, pointRadius, point.GetVelocity(1f));

                circle = RectangleCircle(tileRectangle, circle);

                newPoint.Position = circle.Center;
                newPoint.PreviousPosition = circle.Center - circle.Velocity;
                newPoint.Acceleration = Vector2.Zero;
            }
        }

        return newPoint;
    }

    public static Circle RectangleCircle(Rectangle rectangle, Circle circle)
    {
        float nearestX = MathF.Max(rectangle.X, MathF.Min(circle.Center.X, rectangle.X + rectangle.Width));
        float nearestY = MathF.Max(rectangle.Y, MathF.Min(circle.Center.Y, rectangle.Y + rectangle.Height));

        Vector2 distance = new Vector2(circle.Center.X - nearestX, circle.Center.Y - nearestY);

        if (Vector2.Dot(circle.Velocity, distance) < 0)
        {
            var tangentVelocity = Vector2.Dot(distance.SafeNormalize(Vector2.Zero), circle.Velocity);
            circle.Velocity = circle.Velocity - new Vector2(tangentVelocity * 2);
        }

        float penetrationDepth = circle.Radius - distance.Length();
        Vector2 penetrationVector = distance.SafeNormalize(Vector2.Zero) * penetrationDepth;
        circle.Center = circle.Center - penetrationVector;

        return circle;
    }
}
