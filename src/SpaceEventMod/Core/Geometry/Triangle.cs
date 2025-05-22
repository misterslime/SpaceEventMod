using Microsoft.Xna.Framework;

namespace SpaceEventMod.Core.Geometry;

public class Triangle(Vector2 point1, Vector2 point2, Vector2 point3) : Polygon([point1, point2, point3])
{
}