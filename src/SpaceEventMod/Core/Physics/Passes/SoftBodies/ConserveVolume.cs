using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Components.SoftBodies;
using SpaceEventMod.Core.Physics.Interfaces;
using Terraria;

namespace SpaceEventMod.Core.Physics.Passes.SoftBodies;

[Needs(typeof(GasFilledSoftBody), typeof(PhysicsShape))]
internal class ConserveVolume(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        GasFilledSoftBody body = physicsObject.GetComponent<GasFilledSoftBody>();
        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        float dilation = body.ScaleFactor * (body.DesiredArea - -shape.GetArea());
        Main.NewText(dilation);

        for (int i = 0; i < shape.Points.Length; i++)
        {
            int leftIndex = (i - 1 + shape.Points.Length) % shape.Points.Length;
            int rightIndex = (i + 1) % shape.Points.Length;

            Vector2 point1 = shape.Points[leftIndex].Position;
            Vector2 point2 = shape.Points[rightIndex].Position;

            Vector2 vector = point1 - point2;

            vector = new Vector2(-vector.Y, vector.X).SafeNormalize(Vector2.Zero) * dilation;

            physicsObject.GetComponent<PhysicsShape>().Points[i].Acceleration += vector;
        }
    }
}
