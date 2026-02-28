using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry;
using System;
using Terraria;

namespace SpaceEventMod.Core.Physics.Collision;

internal static class TileCollisionHelper
{
    public static PhysicsPoint CheckPoint(PhysicsPoint point, int sampleSize, float pointRadius)
    {
        var newPoint = point;

        var topLeft = point.Position - 8 * new Vector2(sampleSize);
        var topLeftTiles = topLeft.ToTileCoordinates();

        for (var i = 0; i < sampleSize; i++)
        {
            for (var j = 0; j < sampleSize; j++)
            {
                var tile = Framing.GetTileSafely(topLeftTiles.X + i, topLeftTiles.Y + j);
                if (!tile.active() || !Main.tileSolid[tile.type])
                    continue;

                var tilePosition = new Vector2(topLeftTiles.X + i, topLeftTiles.Y + j) * 16;

                var tileRectangle = new Rectangle((int)tilePosition.X, (int)tilePosition.Y, 16, 16);

                if (!tileRectangle.Contains(point.Position.ToPoint()))
                    continue;

                var circle = new Circle(point.Position, pointRadius, point.GetVelocity(1f));

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
        var nearestX = MathF.Max(rectangle.X, MathF.Min(circle.Center.X, rectangle.X + rectangle.Width));
        var nearestY = MathF.Max(rectangle.Y, MathF.Min(circle.Center.Y, rectangle.Y + rectangle.Height));

        var distance = new Vector2(circle.Center.X - nearestX, circle.Center.Y - nearestY);

        if (Vector2.Dot(circle.Velocity, distance) < 0)
        {
            var tangentVelocity = Vector2.Dot(distance.SafeNormalize(Vector2.Zero), circle.Velocity);
            circle.Velocity = circle.Velocity - new Vector2(tangentVelocity * 2);
        }

        var penetrationDepth = circle.Radius - distance.Length();
        var penetrationVector = distance.SafeNormalize(Vector2.Zero) * penetrationDepth;
        circle.Center = circle.Center - penetrationVector;

        return circle;
    }
}
