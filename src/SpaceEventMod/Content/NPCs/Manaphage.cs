using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Actions.Composite;
using SpaceEventMod.Common.Actions.Decorator;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Common.Actions.Leaf.Attacking;
using SpaceEventMod.Common.Actions.Leaf.Conditions;
using SpaceEventMod.Common.Actions.Leaf.Motion;
using SpaceEventMod.Common.Actions.Leaf.Targeting;
using SpaceEventMod.Content.Dusts;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.Behavior.StateMachines;
using SpaceEventMod.Core.Physics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs;

public class Manaphage : ModNPC, IDynamicMotion, IDynamicStretch, ITimer, ISquidInk
{
    private static BehaviorTree BehaviorTree;

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

    public bool IsSpraying { get; set; }

    public Vector2 CloudPosition { get; set; }

    public Vector2Dynamics SecondOrderSolver { get; set; }

    public Vector2Dynamics Stretching { get; set; }

    public Vector2 TargetStretching { get; set; }

    public FloatDynamics Rotation { get; set; }

    public float TargetRotation { get; set; }

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
            new Sequence(
                new Inverter(new TargetedSquidMovement(15 * 16f, 1f, 60, 15f * 16f))),
                new SprayInkCloud(15f * 16f));

        var findStar = new Sequence(
            new NearStar(60 * 16f),
            new SquidGoToStar(15 * 16f, 1f, 60));

        var wander = new RandomSquidMovement(15 * 16f, 0.35f, 160);

        var root = new Selector(
            new Selector(
                new SprayInkCloudAnimation(),
                new Sequence(entityTargeting, livePhageReaction)
            ),
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
        SecondOrderSolver = new Vector2Dynamics(1f / 128, 0.7f, 0.2f, TargetPosition);
        Mana = MaxMana;

        Stretching = new Vector2Dynamics(1f / 60, 0.5f, 0.5f, Vector2.One);
        TargetStretching = Vector2.One;

        NPC.netUpdate = true;
        NPC.scale = Main.rand.NextFloat(0.8f, 1.1f);

        base.OnSpawn(source);
    }

    public override bool PreAI()
    {
        NPC.rotation = NPC.rotation.AngleLerp((MathF.Abs(SecondOrderSolver.GetVelocity().X) * NPC.direction) / (6 * MathF.Tau), 0.5f);

        BehaviorTree?.Update(NPC.whoAmI);
        NPC.direction = TargetPosition.X >= NPC.Center.X ? 1 : -1;
        NPC.velocity = Vector2.Zero;
        NPC.Center = SecondOrderSolver.Update(1, TargetPosition);

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Vector2 drawPosition = NPC.Center - Main.screenPosition;
        Vector2 stretchFactor = Stretching == null ? Vector2.One : Stretching.Update(1, TargetStretching);

        Main.EntitySpriteDraw(texture, drawPosition, texture.Frame(), NPC.GetAlpha(drawColor), NPC.rotation, texture.Size() * 0.5f, NPC.scale * stretchFactor, 0);

        return false;
    }
}
