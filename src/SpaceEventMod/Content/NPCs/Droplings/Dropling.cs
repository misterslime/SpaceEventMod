using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    Biting = 1
}

public class Dropling : ModNPC
{
    public static readonly SecondOrderDynamics DroplingVelocity = new SecondOrderDynamics(1f / 85f, 0.6f, 0.2f);

    public static readonly SecondOrderDynamics DroplingDeccelerate = new SecondOrderDynamics(1f / 85f, 1f, 0f);

    public static readonly SecondOrderDynamics DroplingDash = new SecondOrderDynamics(1f / 85f, 1f, -1f);

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

    private Vector2 PreviousPosition { get; set; }

    private Vector2 TargetPosition { get; set; }

    private Vector2 TargetVelocity { get; set; }

    private Vector2 Acceleration { get; set; }

    public Kinematics<Vector2> VelocityKinematics
    {
        get => new Kinematics<Vector2>(NPC.velocity, NPC.oldVelocity, Acceleration);
        set
        {
            NPC.velocity = value.Position;
            Acceleration = value.Velocity;
            NPC.oldVelocity = value.PreviousPosition;
        }
    }

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

    public override void SetDefaults()
    {
        NPC.width = 42;
        NPC.height = 46;
        NPC.damage = 0;
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

        State = State switch
        {
            DroplingState.Moving => Moving(),
            DroplingState.Biting => Biting()
        };
    }

    private DroplingState Biting()
    {
        PositionKinematics = DroplingDash.Update(1, PositionKinematics, TargetPosition);
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

    private DroplingState Moving()
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

        var maxSpeed = 4.5f;

        var neighbors = Main.npc.Where(x => x.type == ModContent.NPCType<Dropling>() && x.active && Vector2.Distance(NPC.Center, x.Center) < radius).ToArray();

        var target = (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero) * targetWeight;
        var cohesion = Cohesion(neighbors) * cohesionWeight;
        var separation = Separation(neighbors, separationRadius) * separationWeight;
        var alignment = Alignment(neighbors) * alignmentWeight;
        var surround = Surrounding(neighbors, separationRadius) * surroundWeight;

        if (Timer <= 60f || distance < minBitingDistance)
            target *= -2f;

        var forces = cohesion + separation + alignment + target + surround;

        NPC.rotation = NPC.rotation.AngleLerp(forces.ToRotation(), 0.075f);

        var lineOfSight = Vector2.Dot(forces.SafeNormalize(Vector2.Zero), NPC.rotation.ToRotationVector2()) >= 0.9;

        if (lineOfSight && State == DroplingState.Moving)
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


        var canLunge = true;
        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && DistanceSegmentToPoint(NPC.Center, Main.player[NPC.target].Center, neighbor.Center) < separationRadius)
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
        Acceleration = Vector2.Zero;
        PreviousPosition = NPC.Center - NPC.velocity;
        TargetPosition = Main.player[NPC.target].Center + target * 16f * 1.5f;
        Timer = 0;
        NPC.damage = 10;
        return DroplingState.Biting;

    }

    /// <summary>
    /// Cool method for figuring out if a circle is colliding with a line segment.
    /// From this stackoverflow answer: https://stackoverflow.com/a/1079478
    /// </summary>
    /// <param name="A">Point A of the line segment.</param>
    /// <param name="B">Point B of the line segment.</param>
    /// <param name="C">Point C.</param>
    /// <returns>Returns the distance from line segment AB to point C</returns>
    public float DistanceSegmentToPoint(Vector2 A, Vector2 B, Vector2 C)
    {
        float Hypot2(Vector2 a, Vector2 b) => Vector2.Dot(a - b, a - b);

        // Compute vectors AC and AB
        var AC = C - A;
        var AB = B - A;

        // Get point D by taking the projection of AC onto AB then adding the offset of A
        var D = Project(AC, AB) + A;

        var AD = D - A;

        // D might not be on AB so calculate k of D down AB (aka solve AD = k * AB)
        // We can use either component, but choose larger value to reduce the chance of dividing by zero
        var k = MathF.Abs(AB.X) > MathF.Abs(AB.Y) ? AD.X / AB.X : AD.Y / AB.Y;

        // Check if D is off either end of the line segment
        if (k <= 0.0)
            return MathF.Sqrt(Hypot2(C, A));
        else if (k >= 1.0)
            return MathF.Sqrt(Hypot2(C, B));

        return MathF.Sqrt(Hypot2(C, D));
    }

    // Function for projecting some vector A onto B
    Vector2 Project(Vector2 A, Vector2 B)
    {
        var k = Vector2.Dot(A, B) / Vector2.Dot(B, B);
        return new Vector2(k * B.X, k * B.Y);
    }

    #region Boids Algorithm

    private Vector2 Cohesion(NPC[] neighbors)
    {
        var centerOfMass = NPC.Center;
        var count = 0;

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
        var moveAway = Vector2.Zero;
        var count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && Vector2.Distance(NPC.Center, neighbor.Center) < separationRadius && neighbor.ModNPC is Dropling dropling && dropling.State == DroplingState.Moving)
            {
                var difference = NPC.Center - neighbor.Center;
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
        var averageVelocity = Vector2.Zero;
        var count = 0;

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
        var direction = Vector2.Zero;
        var distanceToNeighbor = float.MaxValue;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.whoAmI != NPC.whoAmI && Vector2.DistanceSquared(neighbor.Center, NPC.Center + NPC.velocity) < distanceToNeighbor)
            {
                distanceToNeighbor = Vector2.DistanceSquared(neighbor.Center, NPC.Center + NPC.velocity);
                direction = NPC.Center - Main.player[NPC.target].Center;
                //direction = NPC.Center - neighbor.Center;
            }
        }

        direction = new Vector2(-direction.Y, direction.X);
        return direction.SafeNormalize(Vector2.Zero);
    }
    #endregion

    public override void OnSpawn(IEntitySource source)
    {
        var values = Enum.GetValues(typeof(DroplingAppendage));
        Appendage = (DroplingAppendage)values.GetValue(Main.rand.Next(values.Length));

        TargetVelocity = Vector2.Zero;
        Acceleration = Vector2.Zero;

        State = DroplingState.Moving;

        Main.NewText(Appendage.ToString());
    }

    public override void FindFrame(int frameHeight)
    {
        var frameWidth = 44;
        var frame = (int)Appendage - 1;

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
        return false;
    }
}
