using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

public struct PhysicsPoint
{
    public PhysicsPoint(Vector2 position, bool isControl = false)
    {
        Position = position;
        PreviousPosition = position;
        IsControl = isControl;
    }

    public bool IsControl { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 PreviousPosition { get; set; }
    public Vector2 Velocity { get; set; } = default;
    public Vector2 Acceleration { get; set; } = default;

    public static explicit operator Vector2(PhysicsPoint value) => value.Position;
}