using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Behavior;
using SpaceEventMod.Core.Physics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Droplings;

public enum DroplingAppendage
{
    None = 0,
    Flagellum = 1,
    BigJaw = 2,
    Wings = 3
}

public enum DroplingState
{
    Moving = 0,
    Arrived = 1
}

public class Dropling : ModNPC
{
    public static readonly Vector2Dynamics DroplingVelocity = new Vector2Dynamics(1f / 85f, 0.6f, 0.2f);

    public static readonly Vector2Dynamics DroplingDeccelerate = new Vector2Dynamics(1f / 85f, 1f, 0f);

    private ref float Timer => ref NPC.ai[1];

    public DroplingState State
    {
        get => (DroplingState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private DroplingAppendage Appendage
    {
        get => (DroplingAppendage)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    private ref float TargetRotation => ref NPC.ai[3];

    private Vector2 TargetVelocity { get; set; }

    private Vector2 Acceleration { get; set; }

    public Kinematics<Vector2> VelocityKinematics
    {
        get => new Kinematics<Vector2>(NPC.velocity, Acceleration).SetPreviousPosition(NPC.oldVelocity);
        set
        {
            NPC.velocity = value.Position;
            Acceleration = value.Velocity;
            NPC.oldVelocity = value.PreviousPosition;
        }
    }

    public override void SetDefaults()
    {
        NPC.width = 42;
        NPC.height = 46;
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

    public override void AI()
    {
        Timer++;

        NPC.TargetClosest(false);

        if (!NPC.HasValidTarget)
            return;

        float cohesionWeight = 1f;
        float separationWeight = 1.5f;
        float alignmentWeight = 1f;
        float targetWeight = 1.5f;
        float surroundWeight = 2f;
        float separationRadius = 48f;
        float radius = 100f;
        float maxSpeed = 4.5f;

        NPC[] neighbors = Main.npc.Where(x => x.type == ModContent.NPCType<Dropling>() && x.active && Vector2.Distance(NPC.Center, x.Center) < radius).ToArray();

        Vector2 target = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero) * targetWeight;
        Vector2 cohesion = Cohesion(neighbors) * cohesionWeight;
        Vector2 separation = Separation(neighbors, separationRadius) * separationWeight;
        Vector2 alignment = Alignment(neighbors) * alignmentWeight;
        Vector2 surround = Surrounding(neighbors, separationRadius) * surroundWeight;

        Vector2 forces = cohesion + separation + alignment + target + surround;

        NPC.rotation = NPC.rotation.AngleLerp(forces.ToRotation(), 0.075f);

        if (Vector2.Dot(forces.SafeNormalize(Vector2.Zero), NPC.rotation.ToRotationVector2()) >= 0.85 && State == DroplingState.Moving)
        {
            TargetVelocity += forces;

            if (TargetVelocity.Length() > maxSpeed)
            {
                TargetVelocity = TargetVelocity.SafeNormalize(Vector2.Zero);
                TargetVelocity *= maxSpeed;
            }

            VelocityKinematics = DroplingVelocity.Update(1, VelocityKinematics, TargetVelocity);
        }
        else
        {
            TargetVelocity = Vector2.Zero;
            VelocityKinematics = DroplingDeccelerate.Update(1, VelocityKinematics, TargetVelocity);
        }

        if (Main.player[NPC.target].getRect().Intersects(NPC.getRect()))
        {
            State = DroplingState.Arrived;
            NPC.velocity = Vector2.Zero;
        }
        else
            State = DroplingState.Moving;
    }

    #region Boids Algorithm

    private Vector2 Cohesion(NPC[] neighbors)
    {
        Vector2 centerOfMass = NPC.Center;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && neighbor.ModNPC is Dropling dropling && dropling.State == DroplingState.Moving)
            {
                centerOfMass += neighbor.Center;
                count++;
            }
        }

        if (count > 0)
        {
            centerOfMass /= count;
            return (centerOfMass - NPC.Center).SafeNormalize(Vector2.Zero);
        }

        return Vector2.Zero;
    }

    private Vector2 Separation(NPC[] neighbors, float separationRadius)
    {
        Vector2 moveAway = Vector2.Zero;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && Vector2.Distance(NPC.Center, neighbor.Center) < separationRadius && neighbor.ModNPC is Dropling dropling && dropling.State == DroplingState.Moving)
            {
                Vector2 difference = NPC.Center - neighbor.Center;
                moveAway += difference.SafeNormalize(Vector2.Zero) / difference.Length();
                count++;
            }
        }

        if (count > 0)
        {
            moveAway /= count;
        }

        return moveAway.SafeNormalize(Vector2.Zero);
    }

    private Vector2 Alignment(NPC[] neighbors)
    {
        Vector2 averageVelocity = Vector2.Zero;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && neighbor.ModNPC is Dropling dropling && dropling.State == DroplingState.Moving)
            {
                averageVelocity += neighbor.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            averageVelocity /= count;
            return averageVelocity.SafeNormalize(Vector2.Zero);
        }

        return Vector2.Zero;
    }

    private Vector2 Surrounding(NPC[] neighbors, float separationRadius)
    {
        Vector2 direction = Vector2.Zero;
        var distanceToNeighbor = float.MaxValue;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && neighbor.ModNPC is Dropling dropling && dropling.State == DroplingState.Arrived && Vector2.DistanceSquared(neighbor.Center, NPC.Center + NPC.velocity) < distanceToNeighbor)
            {
                distanceToNeighbor = Vector2.DistanceSquared(neighbor.Center, NPC.Center + NPC.velocity);
                direction = NPC.Center - neighbor.Center;
            }
        }

        direction = new Vector2(-direction.Y, direction.X);
        return direction.SafeNormalize(Vector2.Zero);
    }
    #endregion

    public override void OnSpawn(IEntitySource source)
    {
        Array values = Enum.GetValues(typeof(DroplingAppendage));
        Appendage = (DroplingAppendage)values.GetValue(Main.rand.Next(values.Length));

        TargetVelocity = Vector2.Zero;
        Acceleration = Vector2.Zero;

        State = DroplingState.Moving;

        Main.NewText(Appendage.ToString());
    }

    public override void FindFrame(int frameHeight)
    {
        int frameWidth = 44;
        int frame = (int)Appendage - 1;

        if (Appendage == DroplingAppendage.None)
            frame = 1;

        NPC.frame.X = frame * frameWidth;
        NPC.frame.Width = frameWidth - 2;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        var drawPosition = NPC.Center - Main.screenPosition;
        var scale = Vector2.One * NPC.scale;
        var origin = new Vector2(NPC.width, NPC.height) * 0.5f;

        Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation + MathHelper.PiOver2, origin, scale, 0);

        var indicator = Assets.Assets.Textures.Indicator.Value;

        var endPosition = drawPosition + NPC.velocity * 15f;

        DrawLine(spriteBatch, drawPosition, endPosition, Color.DarkGray);
        Main.EntitySpriteDraw(indicator, endPosition, null, Color.DarkGray, 0f, indicator.Size() * 0.5f, scale, 0);

        endPosition = drawPosition + TargetVelocity * 15f;

        DrawLine(spriteBatch, drawPosition, endPosition, Color.White);
        Main.EntitySpriteDraw(indicator, endPosition, null, Color.White, 0f, indicator.Size() * 0.5f, scale, 0);

        if (!NPC.HasValidTarget)
            return false;

        endPosition = Main.player[NPC.target].Center - Main.screenPosition;

        Color color = TargetVelocity.Length() == 0f ? Color.Red : Color.Green;

        DrawLine(spriteBatch, drawPosition, endPosition, color);
        Main.EntitySpriteDraw(indicator, endPosition, null, color, 0f, indicator.Size() * 0.5f, scale, 0);
        return false;
    }

    public void DrawLine(SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
    {
        Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
        Vector2 v = Vector2.Normalize(begin - end);
        float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
        if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
        spriteBatch.Draw(Assets.Assets.Textures.WhitePixel.Value, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
    }
}
