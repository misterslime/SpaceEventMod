using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.WorldGeneration;

public struct Sample2D(Vector2 position, float radius)
{
    public Vector2 Position = position;
    public float Radius = radius;

    public override int GetHashCode() => new Vector3(Position.X, Position.Y, Radius).GetHashCode();
}
