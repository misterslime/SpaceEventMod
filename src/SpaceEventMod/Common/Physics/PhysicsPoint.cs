using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics;

public struct PhysicsPoint(Vector2 position) : IPoint
{
    public Vector2 Position { get; set; } = position;
    public Vector2 PreviousPosition { get; set; } = position;
    public Vector2 Acceleration { get; set; } = default;

    public static explicit operator Vector2(PhysicsPoint value) => value.Position;

    public Vector2 GetVelocity(float deltaTime) => (Position - PreviousPosition) / deltaTime;
}