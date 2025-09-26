using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

internal static class Integrators
{
    public static PhysicsPoint VerletIntegration(PhysicsPoint point)
    {
        var previousPosition = point.PreviousPosition;

        point.PreviousPosition = point.Position;
        point.Position = 2 * point.Position - previousPosition + point.Acceleration;
        point.Acceleration = Vector2.Zero;

        return point;
    }

    public static PhysicsPoint SemiImplicitEulerIntegration(PhysicsPoint point)
    {
        point.Velocity += point.Acceleration;
        point.Position += point.Velocity;
        point.Acceleration = Vector2.Zero;

        return point;
    }
}
