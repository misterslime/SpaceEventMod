using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Geometry.Interfaces;

namespace SpaceEventMod.Core.Physics;

public struct PhysicsPoint(Vector2 position) : IPoint
{
    public Vector2 Position { get; set; } = position;
    public Vector2 PreviousPosition { get; set; } = position;
    public Vector2 Acceleration { get; set; } = default;

    public static explicit operator Vector2(PhysicsPoint value) => value.Position;

    public Vector2 GetVelocity(float deltaTime) => (Position - PreviousPosition) / deltaTime;
}