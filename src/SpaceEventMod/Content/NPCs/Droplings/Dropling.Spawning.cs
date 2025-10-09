using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using SpaceEventMod.Core.Physics.Joints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;

namespace SpaceEventMod.Content.NPCs.Droplings;

internal partial class Dropling
{
    public override void OnSpawn(IEntitySource source)
    {
        List<DroplingAppendage> appendages = [DroplingAppendage.Flagellum, DroplingAppendage.Wings, DroplingAppendage.BigJaw];
        Appendage = DroplingAppendage.None;

        do
        {
            Appendage = Appendage | Main.rand.NextFromCollection(appendages);

            appendages.Remove(Appendage);
        }
        while (appendages.Any() && Main.rand.NextBool(4));

        TargetVelocity = Vector2.Zero;

        State = DroplingState.Moving;

        ApplyStats();

        if (!HasAppendage(DroplingAppendage.Flagellum))
            return;

        const float length = 22;
        const int segments = 6;

        ConstructTail(ref _flagellumTail1, Vector2.UnitX, segments, length);
        ConstructTail(ref _flagellumTail2, Vector2.UnitY, segments, length);
        ConstructTail(ref _flagellumTail3, -Vector2.UnitX, segments, length);

        _flagellum = new PhysicsObject(new(NPC.Center));
        _flagellum.AddChild(_flagellumTail1);
        _flagellum.AddChild(_flagellumTail2);
        _flagellum.AddChild(_flagellumTail3);

        List<IJoint> flagellumJoints = new List<IJoint>();

        for (int i = 0; i < 3; i++)
        {
            JointIndex controlIndex = new(IndexType.ObjectPosition, 0);
            JointIndex childIndex = new(IndexType.ChildPosition, i);

            flagellumJoints.Add(new Anchor(controlIndex, childIndex));
        }

        _flagellum.AddComponent(new PhysicsJoints(flagellumJoints.ToArray()));
    }

    private void ApplyStats()
    {
        Speed = 4.5f;
        TurnLerp = 0.1f;

        if (HasAppendage(DroplingAppendage.Wings))
            TurnLerp = 0.20f;

        if (HasAppendage(DroplingAppendage.BigJaw))
            NPC.damage *= 2;

        if (HasAppendage(DroplingAppendage.Flagellum))
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 1.5f);
            NPC.life = NPC.lifeMax;

            Speed = 6f;
        }
    }

    private void ConstructTail(ref PhysicsObject tail, Vector2 start, float segments, float segmentLength)
    {
        List<PhysicsPoint> points = new List<PhysicsPoint>();
        List<IJoint> joints = new List<IJoint>();

        for (int j = 0; j < segments; j++)
        {
            points.Add(new PhysicsPoint(NPC.Center + start * segmentLength * (j + 1)));

            if (j <= 0)
                continue;

            JointIndex index1 = new(IndexType.Point, j - 1);
            JointIndex index2 = new(IndexType.Point, j);

            joints.Add(new DistanceConstraint(index1, index2, segmentLength));
        }

        JointIndex controlIndex = new(IndexType.ObjectPosition, 0);
        JointIndex pointIndex = new(IndexType.Point, 0);

        joints.Add(new DistanceConstraint(controlIndex, pointIndex, segmentLength, true));

        tail = new PhysicsObject(new(NPC.Center));
        tail.AddComponent(new PhysicsShape(points.ToArray()));
        tail.AddComponent(new PhysicsJoints(joints.ToArray()));
        tail.AddComponent(new NPCReference(NPC.whoAmI));
    }
}
