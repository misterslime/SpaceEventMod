using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Composite;
using SpaceEventMod.Common.Actions.Decorator;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Common.Actions.Leaf.Conditions;
using SpaceEventMod.Common.Actions.Leaf.Motion;
using SpaceEventMod.Common.Actions.Leaf.Targeting;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Behavior.StateMachines;
using SpaceEventMod.Core.Physics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs;

public class Manaphage : ModNPC, IDynamicMotion, ITimer, ISquidInk
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

    public int Mana
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    public int MaxMana => 3;

    public int MostRecentCloud { get; set; }

    public Vector2 CloudPosition { get; set; }

    public Vector2Dynamics SecondOrderSolver { get; set; }

    private static BehaviorTree BehaviorTree;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.UsesNewTargetting[Type] = true;

        var entityTargeting = new Selector(
            new HasTarget(),
            new Sequence(
                new Inverter(new NoMana()),
                new AggroAnythingMiningStar(15 * 16f, Type)));

        var livePhageReaction = new Selector(
            new NoMana(),
            //new CloudSprayed(),
            new Sequence(
                //new SprayTargetIfNear(10f * 16f),
                new TargetedSquidMovement(15 * 16f, 1f, 60, 10f * 16f)));

        var findStar = new Sequence(
            new NearStar(60 * 16f),
            new SquidGoToStar(15 * 16f, 1f, 60));

        var wander = new RandomSquidMovement(15 * 16f, 1f, 160);

        var root = new Selector(
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
        SecondOrderSolver = new Vector2Dynamics(1f / 128, 1f, 1f, TargetPosition);
        Mana = MaxMana;

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
