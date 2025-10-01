using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using SpaceEventMod.Core.Physics.Joints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Physics.Passes;

[Needs(typeof(PhysicsJoints))]
internal class JointPhysics(int steps) : IPass
{
    public int Steps { get; init; } = steps;

    public void Pass(PhysicsObject physicsObject)
    {
        PhysicsJoints joints = physicsObject.GetComponent<PhysicsJoints>();

        for (int j = 0; j < joints.Joints.Length; j++)
            UpdateJoint(joints.Joints[j], physicsObject);
    }

    private void UpdateJoint(IJoint joint, PhysicsObject physicsObject)
    {
        if (joint is DistanceConstraint distanceConstraint)
        {
            if (distanceConstraint.Biased)
                BiasedConstrainPoints(distanceConstraint, physicsObject);
            else
                ConstrainPoints(distanceConstraint, physicsObject);
        }
        else if (joint is Anchor anchor)
            AnchorPoints(anchor, physicsObject);
    }

    private void AnchorPoints(Anchor anchor, PhysicsObject physicsObject)
    {
        JointIndex controlIndex = anchor.GetPointIndex(true);
        JointIndex pointIndex = anchor.GetPointIndex(false);

        PhysicsPoint controlPoint = GetJointPoint(controlIndex, physicsObject);

        SetJointPoint(pointIndex, controlPoint, physicsObject);
    }

    private void ConstrainPoints(DistanceConstraint joint, PhysicsObject physicsObject)
    {
        JointIndex point1Index = joint.GetPointIndex(true);
        JointIndex point2Index = joint.GetPointIndex(false);

        PhysicsPoint point1 = GetJointPoint(point1Index, physicsObject);
        PhysicsPoint point2 = GetJointPoint(point2Index, physicsObject);

        var midPoint = (point1.Position + point2.Position) * 0.5f;
        var projection = (point1.Position - point2.Position).SafeNormalize(Vector2.Zero) * joint.TargetDistance * 0.5f;

        point1.Position = midPoint + projection;
        point2.Position = midPoint - projection;

        SetJointPoint(point1Index, point1, physicsObject);
        SetJointPoint(point2Index, point2, physicsObject);
    }

    private void BiasedConstrainPoints(DistanceConstraint joint, PhysicsObject physicsObject)
    {
        JointIndex controlIndex = joint.GetPointIndex(true);
        JointIndex pointIndex = joint.GetPointIndex(false);

        PhysicsPoint physicsPoint = GetJointPoint(pointIndex, physicsObject);
        PhysicsPoint controlPoint = GetJointPoint(controlIndex, physicsObject);

        var projection = (physicsPoint.Position - controlPoint.Position).SafeNormalize(Vector2.Zero) * joint.TargetDistance;

        physicsPoint.Position = controlPoint.Position + projection;

        SetJointPoint(pointIndex, physicsPoint, physicsObject);
    }

    public static PhysicsPoint GetJointPoint(JointIndex jointIndex, PhysicsObject physicsObject)
    {
        return jointIndex.IndexType switch
        {
            IndexType.Point => physicsObject.GetComponent<PhysicsShape>().Points[jointIndex.Index],
            IndexType.PointAverage => throw new NotImplementedException(),
            IndexType.ObjectPosition => physicsObject.Center,
            IndexType.ChildPosition => ((ChildObject)(from component in physicsObject.Components
                                                      where component is ChildObject
                                                      select component)
                                                      .ToArray()[jointIndex.Index]).Child.Center,
        };
    }

    private static void SetJointPoint(JointIndex jointIndex, PhysicsPoint point, PhysicsObject physicsObject)
    {
        switch (jointIndex.IndexType)
        {
            case IndexType.Point:
                physicsObject.GetComponent<PhysicsShape>().Points[jointIndex.Index].Position = point.Position;
                break;
            case IndexType.ObjectPosition:
                physicsObject.Center = point;
                break;
            case IndexType.ChildPosition:
                physicsObject.GetInstancedComponent<ChildObject>(jointIndex.Index).Child.Center = point;
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
