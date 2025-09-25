using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

internal struct EulerPoint(Vector2 initialPosition, Vector2 initialVelocity = default) : IPoint
{
    public Vector2 Position { get; set; } = initialPosition;
    public Vector2 Velocity { get; set; } = initialVelocity;
}
