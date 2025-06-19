using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Common.Actions.NPCs;
using SpaceEventMod.Core.Behavior.Automata;
using SpaceEventMod.Core.GameObjects.Stars;
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
    public PushdownAutomaton<ModNPC> PushdownAutomaton;

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
        PushdownAutomaton = new PushdownAutomaton<ModNPC>(this);

        var randomSquidMovement = new RandomSquidMovement(15 * 16f, 0.35f, 160);
        var squidGoToStar = new SquidGoToStar(15 * 16f, 1f, 60, 15 * 16f);
        var targetedSquidMovement = new TargetedSquidMovement(15 * 16f, 1f, 60);
        var sprayInkCloud = new SprayInkCloud();

        bool CanSprayInk()
        {
            if (!NPC.HasValidTarget)
                return false;

            var targetCenter = NPC.HasNPCTarget ? Main.npc[NPC.TranslatedTargetIndex].Center : Main.player[NPC.TranslatedTargetIndex].Center;

            return targetCenter.WithinRange(NPC.Center, 15f * 16f);
        }

        bool NearStar()
        {
            if (StarSystem.Stars.Count <= 0)
                return false;

            Core.GameObjects.Stars.Star closestStar;
            var distanceToStar = float.MaxValue;

            foreach (var star in StarSystem.Stars)
            {
                if (Vector2.DistanceSquared(star.GetCenter(), NPC.Center) < distanceToStar)
                {
                    distanceToStar = Vector2.DistanceSquared(star.GetCenter(), NPC.Center);
                    closestStar = star;
                }
            }

            if (Math.Sqrt(distanceToStar) <= 60 * 16f)
                return true;

            return false;
        }

        PushdownAutomaton.Add(0, randomSquidMovement)
            .Add(1, squidGoToStar)
            .Add(2, targetedSquidMovement)
            .Add(3, sprayInkCloud)
            .AddTransition(0, 1, NearStar)
            .AddTransition(1, 2, () => NPC.HasValidTarget)
            .AddTransition(1, 3, CanSprayInk)
            .AddTransition(2, 3, CanSprayInk)
            .PushState(0);

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

        PushdownAutomaton.Update();
        NPC.direction = TargetPosition.X >= NPC.Center.X ? 1 : -1;
        NPC.velocity = Vector2.Zero;
        NPC.Center = SecondOrderSolver.Update(1, TargetPosition);

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        var drawPosition = NPC.Center - Main.screenPosition;
        var stretchFactor = Stretching == null ? Vector2.One : Stretching.Update(1, TargetStretching);

        Main.EntitySpriteDraw(texture, drawPosition, texture.Frame(), NPC.GetAlpha(drawColor), NPC.rotation, texture.Size() * 0.5f, NPC.scale * stretchFactor, 0);

        return false;
    }
}
