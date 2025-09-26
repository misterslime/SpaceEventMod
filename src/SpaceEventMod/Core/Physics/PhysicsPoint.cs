using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

[Flags]
internal enum PhysicsPointType
{
    Control = 0b_0000_0000,  // Only Position is active
    PreviousPosition = 0b_0000_0001,
    Velocity = 0b_0000_0010,
    Acceleration = 0b_0000_0100,

    Euler = Velocity,
    Verlet = PreviousPosition | Acceleration,
    Kinematic = PreviousPosition | Velocity
}

internal struct PhysicsPoint(Vector2 position, PhysicsPointType type = PhysicsPointType.Control)
{
    public PhysicsPointType Type { get; set; } = type;
    public Vector2 Position { get; set; } = position;
    public Vector2 PreviousPosition { get; set; } = position;
    public Vector2 Velocity { get; set; } = default;
    public Vector2 Acceleration { get; set; } = default;

    public static implicit operator PhysicsPoint(Vector2 value) => new PhysicsPoint(value);
    public static explicit operator Vector2(PhysicsPoint value) => value.Position;
}
