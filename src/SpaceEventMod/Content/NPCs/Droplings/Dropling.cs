using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.NPCs;
using SpaceEventMod.Common.NPCs.Attributes;
using SpaceEventMod.Core.Animation;
using SpaceEventMod.Core.Animation.SecondOrderDynamics;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Components.Animation;
using SpaceEventMod.Core.Physics.Interfaces;
using SpaceEventMod.Core.Physics.Joints;
using SpaceEventMod.Core.Physics.Passes;
using SpaceEventMod.Core.Physics.Passes.Integrators;
using SpaceEventMod.Core.Physics.Passes.NPCs;
using SpaceEventMod.Core.Utilities;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Droplings;

[Flags]
public enum DroplingAppendage
{
    None       = 0b_0000_0000,
    Flagellum  = 0b_0000_0001,
    BigJaw     = 0b_0000_0010,
    Wings      = 0b_0000_0100
}

public enum DroplingState
{
    Moving = 0,
    Biting = 1
}

internal partial class Dropling : BaseStateNPC<DroplingState>
{
    private static readonly SecondOrderAnimation DroplingVelocity = new SecondOrderAnimation(1f / 85f, 0.6f, 0.2f);

    private static readonly SecondOrderAnimation DroplingDeccelerate = new SecondOrderAnimation(1f / 85f, 1f, 0f);

    private static readonly SecondOrderAnimation DroplingDash = new SecondOrderAnimation(1f / 85f, 1f, -1.5f);

    private static PhysicsSolver s_droplingSolver;

    public DroplingAppendage Appendage
    {
        get => (DroplingAppendage)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    private float _desiredRotation = 0;
    private float _tailRotation = 0;

    public float Speed { get; set; }
    public float TurnLerp { get; set; }

    private bool HasAppendage(DroplingAppendage appendage) => (Appendage & appendage) == appendage;

    private PhysicsObject _flagellum;
    private PhysicsObject _flagellumTail1;
    private PhysicsObject _flagellumTail2;
    private PhysicsObject _flagellumTail3;

    public override void SetStaticDefaults()
    {
        s_droplingSolver = new PhysicsSolver();

        s_droplingSolver.AddPhysicsPass(new TrailNPC());
        s_droplingSolver.AddPhysicsPass(new PointsRepulsion(1));
        s_droplingSolver.AddPhysicsPass(new Gravity(new Vector2(0, 0.5f) / 32, 1));
        s_droplingSolver.AddPhysicsPass(new VerletIntegration());
        s_droplingSolver.AddPhysicsPass(new JointPhysics(8));
        s_droplingSolver.AddPhysicsPass(new DampenVelocity(1));
    }

    public override void SetDefaults()
    {
        NPC.width = 46;
        NPC.height = 42;
        NPC.damage = 50;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void PostAI()
    {
        if (HasAppendage(DroplingAppendage.Flagellum))
        {
            s_droplingSolver.RunPhysicsPasses([_flagellum, _flagellumTail1, _flagellumTail2, _flagellumTail3]);
        }
    }

    [StateProcess<DroplingState>(DroplingState.Biting)]
    public DroplingState Biting()
    {
        NPC.velocity = Vector2.Zero;
        NPC.oldVelocity = Vector2.Zero;

        PhysicsObject physicsObject = new PhysicsObject(PositionPhysics);
        physicsObject.AddComponent(new SecondOrderData(1, DroplingDash, TargetPosition));

        SecondOrderDynamics.Solver.RunPhysicsPasses([physicsObject]);

        PositionPhysics = physicsObject.Center;

        //PositionKinematics = DroplingDash.Update(1, PositionKinematics, TargetPosition);
        NPC.rotation = NPC.rotation.AngleLerp((TargetPosition - NPC.Center).ToRotation(), 0.075f);

        if (!(Timer > 70f))
        {
            return DroplingState.Biting;
        }

        Timer = 0f;
        NPC.knockBackResist = 1f;
        NPC.damage = 0;
        return DroplingState.Moving;

    }

    [StateProcess<DroplingState>(DroplingState.Moving)]
    public DroplingState Moving()
    {
        NPC.TargetClosest(false);

        if (!NPC.HasValidTarget)
            return DroplingState.Moving;

        var cohesionWeight = 1.2f;
        var separationWeight = 1.5f;
        var alignmentWeight = 1.2f;
        var targetWeight = 2f;
        var surroundWeight = 1f;

        var separationRadius = 48f;
        var radius = 20f * 16f;
        var maxBitingDistance = 7.5f * 16f;
        var minBitingDistance = 4f * 16f;
        var distance = Vector2.Distance(Main.player[NPC.target].Center, NPC.Center);

        var neighbors = Main.npc.Where(x => x.type == ModContent.NPCType<Dropling>() && x.active && Vector2.Distance(NPC.Center, x.Center) < radius).ToArray();

        var target = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero) * targetWeight;

        var cohesionChecks = from neighbor in neighbors
                             where neighbor.ModNPC is Dropling dropling && dropling.State == DroplingState.Moving
                             select neighbor;

        var cohesion = NPC.Cohesion(cohesionChecks.ToArray()) * cohesionWeight;
        var separation = NPC.Separation(cohesionChecks.ToArray(), separationRadius) * separationWeight;
        var alignment = NPC.Alignment(cohesionChecks.ToArray()) * alignmentWeight;
        var surround = NPC.Surrounding(neighbors, separationRadius) * surroundWeight;

        if (Timer <= 60f || distance < minBitingDistance)
            target *= -2f;

        var forces = cohesion + separation + alignment + target + surround;

        NPC.rotation = NPC.rotation.AngleLerp(forces.ToRotation(), TurnLerp);

        var lineOfSight = Vector2.Dot(forces.SafeNormalize(Vector2.Zero), NPC.rotation.ToRotationVector2()) >= 0.96;

        if (lineOfSight && State == DroplingState.Moving)
        {
            TargetVelocity += forces;

            if (TargetVelocity.Length() > Speed)
            {
                TargetVelocity = TargetVelocity.SafeNormalize(Vector2.Zero);
                TargetVelocity *= Speed;
            }

            PhysicsObject physicsObject = new PhysicsObject(VelocityPhysics);
            physicsObject.AddComponent(new SecondOrderData(1, DroplingVelocity, TargetVelocity));

            SecondOrderDynamics.Solver.RunPhysicsPasses([physicsObject]);

            VelocityPhysics = physicsObject.Center;

            //VelocityKinematics = DroplingVelocity.Update(1, VelocityKinematics, TargetVelocity);
        }
        else
        {
            PhysicsObject physicsObject = new PhysicsObject(VelocityPhysics);
            physicsObject.AddComponent(new SecondOrderData(1, DroplingDeccelerate, TargetVelocity));

            SecondOrderDynamics.Solver.RunPhysicsPasses([physicsObject]);

            VelocityPhysics = physicsObject.Center;

            TargetVelocity = Vector2.Zero;

            //VelocityKinematics = DroplingDeccelerate.Update(1, VelocityKinematics, TargetVelocity);
        }


        var canLunge = true;
        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && neighbor.Center.DistanceSegmentToPoint(NPC.Center, Main.player[NPC.target].Center) < separationRadius)
            {
                canLunge = false;
            }
        }

        // check if the dropling should continue moving or if it should bite
        var inBiteRange = distance > minBitingDistance && distance <= maxBitingDistance;
        if (!(Timer > 60f) || !canLunge || !lineOfSight || !inBiteRange)
        {
            return DroplingState.Moving;
        }

        NPC.knockBackResist = 0f;
        PreviousPosition = NPC.Center - NPC.velocity;
        TargetPosition = Main.player[NPC.target].Center + target * 16f * 1.5f;
        Timer = 0;
        NPC.damage = 10;
        return DroplingState.Biting;

    }
}