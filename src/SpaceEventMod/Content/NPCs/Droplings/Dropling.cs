using Microsoft.Xna.Framework;
using SpaceEventMod.Common.NPCs;
using SpaceEventMod.Common.NPCs.Attributes;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Content.Items;
using SpaceEventMod.Core.Animation.SecondOrderDynamics;
using SpaceEventMod.Core.Animation.Tweening;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Components.Animation;
using SpaceEventMod.Core.Physics.Passes;
using SpaceEventMod.Core.Physics.Passes.Integrators;
using SpaceEventMod.Core.Physics.Passes.NPCs;
using SpaceEventMod.Core.Utilities;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
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
    private static readonly SecondOrderAnimation s_droplingVelocity = new SecondOrderAnimation(1f / 85f, 0.6f, 0.2f);

    private static readonly SecondOrderAnimation s_droplingDeccelerate = new SecondOrderAnimation(1f / 85f, 1f, 0f);

    private static readonly SecondOrderAnimation s_droplingDash = new SecondOrderAnimation(1f / 15f, 1f, 0);

    private readonly static EasingMotion s_dashMotion = new EasingMotion()
        .SetStart(1f)
        .SetLoops(LoopType.Repeat, 1)
        .ChainMotion(duration: 5f, endValue: 1.5f, Ease.OutSine)
        .ChainMotion(duration: 15f, endValue: 0f, Ease.InBack)
        .ChainMotion(duration: 10f, endValue: -1f, Ease.OutSine)
        .DelayMotion(duration: 15f);

    private static PhysicsSolver s_droplingSolver;

    public DroplingAppendage Appendage
    {
        get => (DroplingAppendage)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    private Vector2 _dashDisplacement;
    private Vector2 _dashTarget;
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
        NPC.knockBackResist = 0f;
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
        float delayBeforeDashing = 50;

        if (Timer <= delayBeforeDashing)
        {
            TargetPosition = Main.player[NPC.target].Center + _dashDisplacement;
            _dashTarget = Main.player[NPC.target].Center;
            _dashDisplacement += NPC.velocity;
            NPC.velocity = NPC.velocity.RotatedBy(MathHelper.PiOver4 * 0.1f * EasingFunctions.OutQuint(Timer / delayBeforeDashing));
            NPC.velocity *= 0.94f;
        }
        else
        {
            float interpolant = s_dashMotion.Evaluate(Timer - delayBeforeDashing, out bool completed);

            Vector2 dashVector = Vector2.Lerp(Vector2.Zero, _dashDisplacement, interpolant);

            if (Timer >= 60f)
            {
                Vector2 dustPosition = Main.rand.NextVector2Circular(16f, 16f);
                dustPosition += NPC.Center;

                Vector2 dustVelocity = Main.rand.NextVector2Circular(2f, 2f) + (PreviousPosition - NPC.Center) / 60;

                var dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<Pixel>(), dustVelocity);
                dust.noGravity = true;
                dust.color = Color.Lerp(Color.Cyan, Color.BlueViolet, Main.rand.NextFloat());
                dust.fadeIn = 70f;
            }

            PreviousPosition = NPC.Center;
            TargetPosition = dashVector + _dashTarget;

            if (completed)
            {
                NPC.Center = TargetPosition;
                VelocityPhysics = default;
                return DroplingState.Moving;
            }
        }

        PhysicsObject physicsObject = new PhysicsObject(PositionPhysics);
        physicsObject.AddComponent(new SecondOrderData(1, s_droplingDash, TargetPosition));

        SecondOrderDynamics.Solver.RunPhysicsPasses([physicsObject]);

        PositionPhysics = physicsObject.Center;

        NPC.rotation = NPC.rotation.AngleLerp(_dashDisplacement.ToRotation() - MathF.PI, 0.1f);
        return DroplingState.Biting;

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
        var maxBitingDistance = 10f * 16f;
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
            physicsObject.AddComponent(new SecondOrderData(1, s_droplingVelocity, TargetVelocity));

            SecondOrderDynamics.Solver.RunPhysicsPasses([physicsObject]);

            VelocityPhysics = physicsObject.Center;

            //VelocityKinematics = DroplingVelocity.Update(1, VelocityKinematics, TargetVelocity);
        }
        else
        {
            PhysicsObject physicsObject = new PhysicsObject(VelocityPhysics);
            physicsObject.AddComponent(new SecondOrderData(1, s_droplingDeccelerate, TargetVelocity));

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
        _dashDisplacement = NPC.Center - Main.player[NPC.target].Center;
        NPC.damage = 10;
        return DroplingState.Biting;

    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jumpjaw>(), chanceDenominator: 8));
    }
}