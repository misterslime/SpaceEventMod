using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Geometry;

internal struct Circle(Vector2 center, float radius, Vector2 velocity = default)
{
    public Vector2 Center { get; set; } = center;
    public Vector2 Velocity { get; set; } = velocity;
    public float Radius { get; set; } = radius;
}
