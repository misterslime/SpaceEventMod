using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.VerletIntegration;

internal struct VerletPoint(Vector2 initialPosition) : IPoint
{
    public Vector2 Position { get; set; } = initialPosition;
    public Vector2 PreviousPosition { get; set; } = initialPosition;
    public Vector2 Acceleration { get; set; }
}
