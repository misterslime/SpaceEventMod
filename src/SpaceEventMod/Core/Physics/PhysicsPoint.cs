using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

internal struct PhysicsPoint(Vector2 position, bool isControl = false)
{
    public bool IsControl { get; set; } = isControl;
    public Vector2 Position { get; set; } = position;
    public Vector2 PreviousPosition { get; set; } = position;
    public Vector2 Velocity { get; set; } = default;
    public Vector2 Acceleration { get; set; } = default;

    public static implicit operator PhysicsPoint(Vector2 value) => new PhysicsPoint(value);
    public static explicit operator Vector2(PhysicsPoint value) => value.Position;
}
