using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Common.Actions.Composite;
using SpaceEventMod.Common.Actions.Leaf.Conditions;
using SpaceEventMod.Common.Actions.Leaf.Targeting;
using SpaceEventMod.Common.Actions.Leaf;
using SpaceEventMod.Common.Actions.Leaf.Motion;
using SpaceEventMod.Common.Actions.Decorator;
using System;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using Terraria.GameContent;

namespace SpaceEventMod.Content.NPCs;

public class Manaphage : ModNPC, IDynamicMotion, ITimer, ISquidIdleGravity
{
    public Vector2 TargetPosition
    {
        get => new Vector2(NPC.ai[0], NPC.ai[1]);
        set
        {
            NPC.ai[0] = value.X;
            NPC.ai[1] = value.Y;
        }
    }

    public int Time
    {
        get => (int)NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    public float Gravity
    {
        get => NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    public int JellyfishAnimationTime;

    public Vector2Dynamics SecondOrderSolver { get; set; }

    private static BehaviorTree BehaviorTree;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.UsesNewTargetting[Type] = true;

        Selector entityTargeting = new Selector(
            new HasTarget(),
            new Sequence(
                new Inverter(new LowHealth(0.3333f)),
                new AggroAnythingMiningStar(15 * 16f, Type)));

        Selector livePhageReaction = new Selector(
            new Sequence(
                new LowHealth(0.3333f),
                new TargetedSquidMovement(30 * 16f, 1f, 40, 40f * 16f, false)),
            new TargetedSquidMovement(40 * 16f, 1f, 60, 10f * 16f));

        Sequence findStar = new Sequence(
            new StarNearby(60 * 16f),
            new NearestStarSquidMovement(20 * 16f, 1f, 60));

        RandomSquidMovement wander = new RandomSquidMovement(20 * 16f, 1f, 480);

        Selector root = new Selector(
            new Sequence(
                entityTargeting,
                livePhageReaction),
            findStar,
            wander);

        BehaviorTree = new BehaviorTree(root);
    }

    public override void SetDefaults()
    {
        NPC.width = 34;
        NPC.height = 58;
        NPC.damage = 25;
        NPC.defense = 3;
        NPC.lifeMax = 90;
        NPC.knockBackResist = 0.8f;
        NPC.value = 9f;
        NPC.HitSound = SoundID.NPCHit25;
        NPC.DeathSound = SoundID.NPCDeath25;
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        TargetPosition = NPC.Center;
        SecondOrderSolver = new Vector2Dynamics(1f / 512f, 1f, 1f, TargetPosition);

        NPC.netUpdate = true;

        base.OnSpawn(source);
    }

    public override bool PreAI()
    {
        BehaviorTree?.Update(NPC.whoAmI);
        NPC.direction = TargetPosition.X >= NPC.Center.X ? 1 : -1;
        NPC.velocity = Vector2.Zero;
        NPC.Center = SecondOrderSolver.Update(1, TargetPosition);
        NPC.rotation = (MathF.Abs(SecondOrderSolver.GetVelocity().X) * NPC.direction) / (6 * MathF.Tau);
        return false;
    }
}
