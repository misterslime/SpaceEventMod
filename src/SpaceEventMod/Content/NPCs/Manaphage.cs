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
using Star = SpaceEventMod.Core.GameObjects.Stars.Star;

namespace SpaceEventMod.Content.NPCs;

public class Manaphage : ModNPC, IMovement, IWantStar
{
    public static StateMachine<ModNPC> StateMachine;

    public PushdownAutomaton<ModNPC> PushdownAutomaton;

    public Vector2 CloudPosition
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

    public Vector2 TargetPosition { get; set; }

    public int MaxMana => 3;

    public bool IsSpraying { get; set; }

    public Vector2Dynamics SecondOrderSolver { get; set; }

    public Vector2Dynamics Stretching { get; set; }

    public Vector2 TargetStretching { get; set; }

    public FloatDynamics VisualRotationSolver { get; set; }

    public Star ObservedStar { get; set; }

    public override void SetStaticDefaults()
    {
        NPCID.Sets.UsesNewTargetting[Type] = true;

        StateMachine = new StateMachine<ModNPC>();

        var wander = new MovementRandomJitter(15 * 16f, 160);
        var goToStar = new MovementToStar(15 * 16f, 120);
        var chasePlayer = new MovementTowardsTarget(true, 15 * 16f, 60);
        var sprayInkCloud = new SprayInkCloud();

        bool CanSprayInk(ModNPC modNPC)
        {
            var npc = modNPC.NPC;

            if (!npc.HasValidTarget)
                return false;

            var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

            return targetCenter.WithinRange(npc.Center, 15f * 16f);
        }

        bool NearStar(ModNPC modNPC)
        {
            if (StarSystem.Stars.Count <= 0)
                return false;

            var npc = modNPC.NPC;

            Star closestStar;
            var distanceToStar = float.MaxValue;

            foreach (var star in StarSystem.Stars)
            {
                if (Vector2.DistanceSquared(star.GetCenter(), npc.Center) < distanceToStar)
                {
                    distanceToStar = Vector2.DistanceSquared(star.GetCenter(), npc.Center);
                    closestStar = star;
                }
            }

            if (Math.Sqrt(distanceToStar) <= 60 * 16f)
                return true;

            return false;
        }

        StateMachine.Add(0, wander)
            .Add(1, goToStar)
            .Add(2, chasePlayer)
            .Add(3, sprayInkCloud)
            .AddTransition(-1, 0, (modNPC) => true) // always add a transition between -1 and the default when using pushdown automata otherwise it will crash
            .AddTransition(0, 1, NearStar)
            .AddTransition(0, 2, (modNPC) => modNPC.NPC.HasValidTarget)
            .AddTransition(0, 3, CanSprayInk)
            .AddTransition(1, 2, (modNPC) => modNPC.NPC.HasValidTarget)
            .AddTransition(1, 3, CanSprayInk)
            .AddTransition(2, 3, CanSprayInk);
    }

    public override void Unload()
    {
        StateMachine = null;
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
        PushdownAutomaton = new PushdownAutomaton<ModNPC>();
        PushdownAutomaton.PushState(0);

        TargetPosition = NPC.Center;
        SecondOrderSolver = new Vector2Dynamics(1f / 128, 0.7f, 0.2f, TargetPosition);
        Mana = MaxMana;

        Stretching = new Vector2Dynamics(1f / 60, 0.5f, 0.5f, Vector2.One);
        TargetStretching = Vector2.One;

        NPC.rotation = 0f;
        VisualRotationSolver = new FloatDynamics(1f / 30, 1f, 1f, NPC.rotation);

        NPC.scale = Main.rand.NextFloat(0.8f, 1.1f);
        NPC.netUpdate = true;

        base.OnSpawn(source);
    }

    public void EntityMovement(Vector2 motionVector, params float[] arguments)
    {
        motionVector.Normalize();
        float jumpDistance = arguments[0];
        int cooldown = (int)arguments[1];

        if (Time > 0)
        {
            Time--;

            if (TargetPosition.Distance(NPC.Center) <= 16)
                TargetPosition += new Vector2(0, 0.35f);

            if (Time < 15)
                TargetStretching = new Vector2(1.1f, 0.75f);
            else if (Time >= cooldown - 5)
                TargetStretching = new Vector2(0.8f, 1.25f);
            else
                TargetStretching = Vector2.One;

            return;
        }

        Time = cooldown;

        TargetPosition += motionVector * jumpDistance;
        NPC.direction = TargetPosition.X >= NPC.Center.X ? 1 : -1;

        NPC.netUpdate = true;
    }

    public override bool PreAI()
    {
        NPC.rotation = NPC.rotation.AngleLerp((MathF.Abs(SecondOrderSolver.GetVelocity().X) * NPC.direction) / (6 * MathF.PI), 0.95f);

        PushdownAutomaton.Update(this, StateMachine);
        NPC.Center = SecondOrderSolver.Update(1, TargetPosition);

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        var drawPosition = NPC.Center - Main.screenPosition;
        var stretchFactor = Stretching?.Update(1, TargetStretching) ?? Vector2.One;
        var rotation = VisualRotationSolver?.Update(1, NPC.rotation) ?? NPC.rotation;

        Main.EntitySpriteDraw(texture, drawPosition, texture.Frame(), NPC.GetAlpha(drawColor), rotation, texture.Size() * 0.5f, NPC.scale * stretchFactor, 0);

        return false;
    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        NPC.target = player.whoAmI;
        NPC.targetRect = Main.player[player.whoAmI].getRect();
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        NPC.target = projectile.owner;
        NPC.targetRect = Main.player[projectile.owner].getRect();
    }
}
