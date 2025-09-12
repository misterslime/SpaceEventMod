using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Common.Actions.NPCs;
using SpaceEventMod.Content.Events.Space.LevelElements;
using SpaceEventMod.Core.Behavior.Automata;
using SpaceEventMod.Core.Physics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Star = SpaceEventMod.Content.Events.Space.LevelElements.Star;

namespace SpaceEventMod.Content.NPCs.Manaphages;

public class Manaphage : ModNPC, IMovement, IWantStar, ITimer
{
    public static StateMachine<ModNPC> StateMachine;

    public static readonly SecondOrderDynamics PositionSolver = new SecondOrderDynamics(1f / 128f, 0.85f, 0.2f);

    public static readonly SecondOrderDynamics StretchSolver = new SecondOrderDynamics(1f / 40f, 0.3f, -0.5f);

    #region Fields and properties
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

    public Kinematics<Vector2> PositionKinematics
    {
        get => new Kinematics<Vector2>(NPC.Center, PreviousPosition, NPC.velocity);
        set
        {
            NPC.Center = value.Position;
            NPC.velocity = value.Velocity;
            PreviousPosition = value.PreviousPosition;
        }
    }

    public Vector2 PreviousPosition { get; set; }

    public Kinematics<Vector2> StretchingKinematics { get; set; }

    public int MaxMana => 3;

    public bool IsSpraying { get; set; }

    public bool Spawned { get; set; } = false;

    public Vector2 TargetStretching { get; set; }

    public Star ObservedStar { get; set; }

    public Vector2 RelativePosition { get; set; }
    #endregion

    private enum ManaphageStates
    {
        Wander,
        GoToStar,
        DrinkStar,
        ChasePlayer,
        SprayInkCloud,
        SpitInk,
        Sleeping
    }

    public override void SetStaticDefaults()
    {
        NPCID.Sets.UsesNewTargetting[Type] = true;

        StateMachine = new StateMachine<ModNPC>();

        var wander = new MovementRandomJitter();
        var goToStar = new MovementToStar();
        var chasePlayer = new MovementTowardsTarget(true);
        var drinkStar = new DrinkStar();
        var sprayInkCloud = new ManaphageSprayInkCloud();
        var spitInk = new ManaphageInkSpit();
        var sleeping = new ManaphageSleepingRegen();

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
            if (Stars.List.Count <= 0)
                return false;

            var npc = modNPC.NPC;

            Star closestStar;
            var distanceToStar = float.MaxValue;

            foreach (var star in Stars.List)
            {
                if (Vector2.DistanceSquared(star.GetCenter(), npc.Center) < distanceToStar)
                {
                    distanceToStar = Vector2.DistanceSquared(star.GetCenter(), npc.Center);
                    closestStar = star;
                }
            }

            return Math.Sqrt(distanceToStar) <= 60 * 16f;
        }

        bool OnStar(ModNPC modNPC)
        {
            if (modNPC is not IWantStar wantStar)
                throw new Exception("ModNPC is not IWantStar.");

            if (Stars.List.Count <= 0 || !Stars.List.Contains(wantStar.ObservedStar))
                return false;

            return modNPC.NPC.getRect().Intersects(wantStar.ObservedStar.GetBoundingBox());
        }

        bool CanSleep(ModNPC modNPC)
        {
            var npc = modNPC.NPC;

            return npc.life < npc.lifeMax * 0.5 && !npc.HasValidTarget;
        }

        StateMachine
            .AddState((int)ManaphageStates.Wander, wander)
            .AddState((int)ManaphageStates.GoToStar, goToStar)
            .AddState((int)ManaphageStates.DrinkStar, drinkStar)
            .AddState((int)ManaphageStates.ChasePlayer, chasePlayer)
            .AddState((int)ManaphageStates.SprayInkCloud, sprayInkCloud)
            .AddState((int)ManaphageStates.SpitInk, spitInk)
            .AddState((int)ManaphageStates.Sleeping, sleeping)
            .AddTransition(-1,                               (int)ManaphageStates.Wander,        (modNPC) => true) // always add a transition between -1 and the default when using pushdown automata otherwise it will crash
            .AddTransition((int)ManaphageStates.Wander,      (int)ManaphageStates.GoToStar,      NearStar)
            .AddTransition((int)ManaphageStates.Wander,      (int)ManaphageStates.ChasePlayer,   (modNPC) => modNPC.NPC.HasValidTarget)
            .AddTransition((int)ManaphageStates.Wander,      (int)ManaphageStates.SprayInkCloud, CanSprayInk)
            .AddTransition((int)ManaphageStates.Wander,      (int)ManaphageStates.Sleeping,      CanSleep)
            .AddTransition((int)ManaphageStates.GoToStar,    (int)ManaphageStates.DrinkStar,     OnStar)
            .AddTransition((int)ManaphageStates.GoToStar,    (int)ManaphageStates.ChasePlayer,   (modNPC) => modNPC.NPC.HasValidTarget)
            .AddTransition((int)ManaphageStates.GoToStar,    (int)ManaphageStates.SprayInkCloud, CanSprayInk)
            .AddTransition((int)ManaphageStates.GoToStar,    (int)ManaphageStates.Sleeping,      CanSleep)
            .AddTransition((int)ManaphageStates.DrinkStar,   (int)ManaphageStates.ChasePlayer,   (modNPC) => modNPC.NPC.HasValidTarget)
            .AddTransition((int)ManaphageStates.DrinkStar,   (int)ManaphageStates.SprayInkCloud, CanSprayInk)
            .AddTransition((int)ManaphageStates.ChasePlayer, (int)ManaphageStates.SprayInkCloud, CanSprayInk)
            .AddTransition((int)ManaphageStates.ChasePlayer, (int)ManaphageStates.SpitInk,       (modNPC) => Main.rand.NextBool(240))
            .AddTransition((int)ManaphageStates.Sleeping,    (int)ManaphageStates.ChasePlayer,   (modNPC) => modNPC.NPC.HasValidTarget)
            .AddTransition((int)ManaphageStates.Sleeping,    (int)ManaphageStates.SprayInkCloud, CanSprayInk);
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
        Mana = MaxMana;

        TargetStretching = Vector2.One;
        StretchingKinematics = new Kinematics<Vector2>(Vector2.One);

        NPC.rotation = 0f;

        NPC.scale = Main.rand.NextFloat(0.8f, 1.1f);
        NPC.netUpdate = true;

        PreviousPosition = NPC.position;

        base.OnSpawn(source);
    }

    public override bool PreAI()
    {
        PushdownAutomaton.Update(this, StateMachine);
        return false;
    }

    public void EntityMovement(Vector2 motionVector, params float[] arguments)
    {
        motionVector = motionVector.RotatedByRandom(0.6);
        motionVector.Normalize();
        float jumpDistance = 15 * 16f;
        int cooldown = 160;

        if (NPC.HasValidTarget)
            cooldown = 100;
        else if (Stars.List.Contains(ObservedStar))
            cooldown = 120;

        Time--;

        if (TargetPosition.Distance(NPC.Center) <= 16)
            TargetPosition += new Vector2(0, 0.35f);

        if (Time < 15)
            TargetStretching = new Vector2(1.1f, 0.75f);
        else if (Time >= cooldown - 5)
            TargetStretching = new Vector2(0.8f, 1.25f);
        else
            TargetStretching = Vector2.One;

        if (Time <= 0)
        {
            Time = cooldown;

            TargetPosition += motionVector * jumpDistance;
            NPC.direction = TargetPosition.X >= NPC.Center.X ? 1 : -1;

            NPC.netUpdate = true;
        }

        PositionKinematics = PositionSolver.Update(1, PositionKinematics, TargetPosition);
        NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.X / (6 * MathF.PI), 0.95f);
    }

    public bool DrinkAnimation()
    {
        var starPosition = ObservedStar.GetCenter();

        Vector2 toStar = starPosition + ObservedStar.SpriteDisplacement - NPC.Center;
        toStar.Normalize();

        NPC.rotation = NPC.rotation.AngleLerp(toStar.ToRotation() + ObservedStar.Rotation - MathHelper.PiOver2, 0.95f);

        NPC.Center = ObservedStar.GetCenter() + ObservedStar.SpriteDisplacement + RelativePosition.RotatedBy(ObservedStar.Rotation);

        TargetStretching = Vector2.One;

        return true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        var drawPosition = NPC.Center - Main.screenPosition;
        var scale = Vector2.One;
        StretchingKinematics = StretchSolver.Update(1, StretchingKinematics, TargetStretching);

        Main.EntitySpriteDraw(texture, drawPosition, texture.Frame(), NPC.GetAlpha(drawColor), NPC.rotation, texture.Size() * 0.5f, NPC.scale * StretchingKinematics.Position, 0);

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
