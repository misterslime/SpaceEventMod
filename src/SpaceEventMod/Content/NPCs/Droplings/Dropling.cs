using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Animation;
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

public class Dropling : ModNPC
{
    public static readonly SecondOrderDynamics DroplingVelocity = new SecondOrderDynamics(1f / 85f, 0.6f, 0.2f);

    public static readonly SecondOrderDynamics DroplingDeccelerate = new SecondOrderDynamics(1f / 85f, 1f, 0f);

    public static readonly SecondOrderDynamics DroplingDash = new SecondOrderDynamics(1f / 120, 1f, -1.5f);

    public static readonly SecondOrderDynamics DroplingWingAngle = new SecondOrderDynamics(1f / 30, 0.8f, 0f);

    private static PhysicsSolver s_physicsSolver;

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

    private float _desiredRotation = 0;
    private float _tailRotation = 0;
    private float _wiggleTimer = 0;

    private bool HasAppendage(DroplingAppendage appendage) => (Appendage & appendage) == appendage;

    private PhysicsData _flagellum;

    public override void SetStaticDefaults()
    {
        s_physicsSolver = new PhysicsSolver(Integrators.VerletIntegration)
            .AddGlobalData(
                ("gravity", new Vector2(0, 0.025f)))
            .AddPhysicsPass(
                ("pushedByNPC", true, 1, (PhysicsPoint point, SimulationContext context) =>
                {
                    point.Acceleration -= NPC.velocity / 2048;

                    return point;
                }), 
                ("tailEndRepulsion", true, 1, (PhysicsPoint point, SimulationContext context) =>
                {
                    for (int i = 0; i < context.LocalData.PointCount; i++)
                    {
                        if (context.Index != i)
                        {
                            Vector2 vector = point.Position - context.LocalData.GetPoint(i).Position;

                            if (vector.Length() < 32f)
                                point.Acceleration += (vector.SafeNormalize(Vector2.Zero) * 0.015f) / vector.Length();
                        }
                    }

                    return point;
                }),
                ("gravity", true, 1, (PhysicsPoint point, SimulationContext context) =>
                {
                    point.Acceleration += context.GlobalData["gravity"].Vector2 / 32;

                    return point;
                }),
                ("dampenVelocity", true, 1, (PhysicsPoint point, SimulationContext context) =>
                {
                    point.Acceleration -= (point.Position - point.PreviousPosition) * 0.1f * 0.125f;

                    return point;
                }));
    }

    public override void SetDefaults()
    {
        NPC.width = 46;
        NPC.height = 42;
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

    public override void OnSpawn(IEntitySource source)
    {
        Appendage = (DroplingAppendage)Main.rand.Next(0, 8);

        TargetVelocity = Vector2.Zero;
        Acceleration = Vector2.Zero;

        State = DroplingState.Moving;

        Main.NewText(Appendage.ToString());

        if (!HasAppendage(DroplingAppendage.Flagellum))
            return;

        float length = 22;
        int segments = 6;

        _flagellum = new PhysicsData()
            .AddLocalData(
                ("droplingTail", NPC.Center),
                ("segmentsPerTail", segments));

        for (int i = 0; i < 3; i++)
        {
            Vector2 startVector = i switch
            {
                0 => Vector2.UnitY,
                1 => Vector2.UnitX,
                2 => -Vector2.UnitX
            };

            for (int j = 0; j < segments; j++)
            {
                _flagellum.AddPoint(new PhysicsPoint(NPC.Center + startVector * length * (j + 1)));
            }

            _flagellum.AddLink("droplingTail", i * segments, length);
        }

        for (int i = 0; i < 3; i++)
        {
            int count = _flagellum.PointCount / 3;

            for (int j = 1; j < count; j++)
            {
                _flagellum.AddLink(count * i + j - 1, count * i + j, length);
            }
        }
    }

    public override void AI()
    {
        Timer++;

        State = State switch
        {
            DroplingState.Moving => Moving(),
            DroplingState.Biting => Biting()
        };

        if (_flagellum is not null)
        {
            _flagellum.SetPoint("droplingTail", NPC.Center);
            s_physicsSolver.RunSimulation(in _flagellum);
        }
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

        NPC.rotation = NPC.rotation.AngleLerp(forces.ToRotation(), 0.125f);

        var lineOfSight = Vector2.Dot(forces.SafeNormalize(Vector2.Zero), NPC.rotation.ToRotationVector2()) >= 0.96;

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
        Acceleration = Vector2.Zero;
        PreviousPosition = NPC.Center - NPC.velocity;
        TargetPosition = Main.player[NPC.target].Center + target * 16f * 1.5f;
        Timer = 0;
        NPC.damage = 10;
        return DroplingState.Biting;

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

    private int AppendageFrame()
    {
        var frame = 1;

        if (HasAppendage(DroplingAppendage.Wings))
            frame = 2;

        if (HasAppendage(DroplingAppendage.Flagellum))
            frame = 0;

        if (HasAppendage(DroplingAppendage.Wings) && HasAppendage(DroplingAppendage.Flagellum))
            frame = 3;

        return frame;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        _tailRotation = _tailRotation.AngleLerp(NPC.rotation, 0.075f);

        var texture = TextureAssets.Npc[Type].Value;
        var drawPosition = NPC.Center;
        var scale = Vector2.One * NPC.scale;
        var origin = new Vector2(NPC.width, NPC.height) * 0.5f;

        var headPosition = NPC.rotation.ToRotationVector2();
        var tailPosition = _tailRotation.ToRotationVector2();

        var segments = 20;
        var trailPoints = new List<Vector2>(segments + 1);

        var bendiness = Vector2.Dot(headPosition, tailPosition);
        var midPoint = _tailRotation.AngleLerp(NPC.rotation + MathF.PI, 0.5f).ToRotationVector2() * (1 - bendiness) * NPC.height * 0.5f;
        drawPosition += midPoint;

        headPosition *= NPC.width * 0.5f;
        headPosition = drawPosition + headPosition;

        tailPosition *= NPC.width * 0.5f;
        tailPosition = drawPosition - tailPosition;

        ReadOnlySpan<Vector2> controlPoints = new Vector2[] { headPosition, drawPosition, tailPosition };
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(segments + 1);

        var wiggleStrength = Math.Clamp(NPC.velocity.Length() * 0.5f, 1, 4);
        var sineLimit = (3.5f * MathF.PI) / 2;

        Vector2[] trailPointsArray = ApplyWiggleToPoints(in trailPoints, wiggleStrength, sineLimit, bendiness);

        Pipeline pipeline = Graphics.BeginPipeline();

        if (HasAppendage(DroplingAppendage.Wings))
        {
            DrawWings(in pipeline, in trailPointsArray, trailPoints[7], screenPos, drawColor);
        }

        pipeline
            .DrawTrail(
                trailPointsArray,
                _ => NPC.height,
                _ => drawColor,
                Assets.Assets.Shaders.Trail.BendyTexture.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("sampleTexture", texture),
                ("frame", new Vector4(0, (float)AppendageFrame(), 1, 4)));

        if (HasAppendage(DroplingAppendage.Wings))
        {
            DrawWings(in pipeline, in trailPointsArray, trailPoints[7], screenPos, drawColor, true);
        }

        Texture2D starTexture = Assets.Assets.Textures.NPCs.Droplings.DroplingStar.Value;
        Texture2D starGlowTexture = Assets.Assets.Textures.NPCs.Droplings.DroplingStar_Glow.Value;

        Vector2 starPositionDifferenceFromCenter = trailPoints[7] - drawPosition;
        starPositionDifferenceFromCenter *= 0.25f;

        Vector2 starPosition = drawPosition + starPositionDifferenceFromCenter + new Vector2(4, 0).RotatedBy(NPC.rotation);

        // fuck my life
        float time = (NPC.whoAmI * 0.13f + Main.GlobalTimeWrappedHourly) % 1.35f;
        float heartbeat = 0;

        if (time <= 0.15)
            heartbeat = EasingFunctions.QuintEaseInOut(time / 0.15f);
        else if (time <= 0.15 + 0.15)
            heartbeat = 1 - EasingFunctions.CircEaseIn((time - 0.15f) / 0.15f);
        else if (time <= 0.15 + 0.15 + 0.1)
            heartbeat = 0.20f;
        else if (time <= 0.15 + 0.15 + 0.1 + 0.15)
            heartbeat = 0.20f + 0.55f * EasingFunctions.SineEaseIn((time - 0.15f - 0.15f - 0.1f) / 0.15f);
        else if (time <= 0.15 + 0.15 + 0.1 + 0.15 + 0.15)
            heartbeat = 0.75f - 0.75f * EasingFunctions.CircEaseIn((time - 0.15f - 0.15f - 0.1f - 0.15f) / 0.15f);
        else if (time <= 0.15 + 0.15 + 0.1 + 0.15 + 0.15 + 0.65)
            heartbeat = 0;

        float starScale = 0.75f + 0.75f * heartbeat;
        float starRotation = (trailPoints[7] - trailPoints[8]).ToRotation();

        Color starColor = Color.Cyan;
        starColor.A = 0;

        spriteBatch.Draw(starGlowTexture, starPosition - screenPos, null, starColor, starRotation, starGlowTexture.Size() * 0.5f, NPC.scale * starScale * 0.95f, 0, 0);
        spriteBatch.Draw(starTexture, starPosition - screenPos, null, Color.White, starRotation, starTexture.Size() * 0.5f, NPC.scale * starScale, 0, 0);

        /*Graphics.BeginPipeline()
            .DrawSprite(
                starGlowTexture,
                starPosition - screenPos,
                starColor,
                null,
                starRotation,
                starGlowTexture.Size() * 0.5f,
                new Vector2(NPC.scale * starScale))
            .DrawSprite(
                starTexture,
                starPosition - screenPos,
                Color.White,
                null,
                starRotation,
                starTexture.Size() * 0.5f, 
                new Vector2(NPC.scale * starScale))
            .Schedule(RenderLayer.AfterNPCs);*/

        Texture2D jawsTexture = Assets.Assets.Textures.NPCs.Droplings.DroplingJaw.Value;
        Texture2D bigJawsTexture = Assets.Assets.Textures.NPCs.Droplings.DroplingJawBig.Value;

        var jawTextureOrigins = new Dictionary<string, Vector2>
        {
            { "jaw", new Vector2(13, 12) },
            { "bigJaw", new Vector2(17, 30) }
        };

        Vector2 jawPosition = trailPoints[0] - screenPos;
        float jawRotation = MathHelper.WrapAngle((trailPoints[1] - trailPoints[0]).ToRotation() - MathHelper.PiOver2);

        if (HasAppendage(DroplingAppendage.BigJaw))
            spriteBatch.Draw(bigJawsTexture, jawPosition, null, drawColor, jawRotation, jawTextureOrigins["bigJaw"], NPC.scale, 0, 0);
        else
            spriteBatch.Draw(jawsTexture, jawPosition, null, drawColor, jawRotation, jawTextureOrigins["jaw"], NPC.scale, 0, 0);

        DrawTail(in pipeline, in spriteBatch, Assets.Assets.Textures.NPCs.Droplings.DroplingTentacle2.Value, screenPos, drawColor);

        pipeline
            .ApplyOutline(new Color(23, 23, 130))
            .ApplyOutline(new Color(23, 23, 130))
            .Schedule(RenderLayer.AfterNPCs);

        return false;
    }

    private Vector2[] ApplyWiggleToPoints(in List<Vector2> trailPoints, float wiggleStrength, float sineLimit, float bendiness)
    {
        var newTrailPoints = trailPoints;

        _wiggleTimer += 1 / 8f + wiggleStrength / 30f;

        for (var i = 0; i < trailPoints.Count; i++)
        {
            if (i < trailPoints.Count - 1)
            {
                var point = trailPoints[i];
                var nextPoint = trailPoints[i + 1];

                var displacement = point - nextPoint;
                displacement = new Vector2(-displacement.Y, displacement.X).SafeNormalize(Vector2.Zero);

                newTrailPoints[i] = point + displacement * wiggleStrength * (float)Math.Clamp(bendiness, 0.5, 1) * MathF.Sin(((sineLimit * i) / trailPoints.Count) + _wiggleTimer);
            }
            else
            {
                var point = trailPoints[i];
                var previousPoint = trailPoints[i - 1];

                var displacement = previousPoint - point;
                displacement = new Vector2(-displacement.Y, displacement.X).SafeNormalize(Vector2.Zero);

                newTrailPoints[i] = point + displacement * wiggleStrength * (float)Math.Clamp(bendiness, 0.5, 1) * MathF.Sin(((sineLimit * i) / trailPoints.Count) + _wiggleTimer);
            }
        }

        return newTrailPoints.ToArray();
    }

    private void DrawTail(in Pipeline pipeline, in SpriteBatch spriteBatch, Texture2D texture, Vector2 screenPos, Color drawColor)
    {
        if (_flagellum is null)
            return;

        for (int i = 0; i < _flagellum.PointCount; i++)
        {
            PhysicsPoint point = _flagellum.GetPoint(i);
            float rotation = (NPC.Center - point.Position).ToRotation();

            if (i % 8 != 0)
                rotation = (_flagellum.GetPoint(i - 1).Position - point.Position).ToRotation();

            //spriteBatch.Draw(texture, point.Position - screenPos, null, drawColor, rotation + MathHelper.PiOver2, new Vector2(5, 22), NPC.scale, 0, 0);

            pipeline.DrawSprite(texture, point.Position - screenPos, drawColor, null, rotation + MathHelper.PiOver2, new Vector2(5, 22), new Vector2(NPC.scale), 0);
        }
    }

    private void DrawWings(in Pipeline pipeline, in Vector2[] trailPoints, Vector2 drawPosition, Vector2 screenPos, Color drawColor, bool drawingGlow = false)
    {
        Texture2D wingTexture = Assets.Assets.Textures.NPCs.Droplings.DroplingWing.Value;
        Texture2D glowTexture = Assets.Assets.Textures.NPCs.Droplings.DroplingWing_Glow.Value;

        Vector2 wingPosition = new Vector2(6, 26);

        float wingRotation = MathHelper.WrapAngle((trailPoints[5] - trailPoints[4]).ToRotation());

        _desiredRotation = _desiredRotation.AngleLerp(NPC.velocity.Length() * (State == DroplingState.Biting ? 0.30f : 0.08f), 0.1f);

        float leftWingRotation = MathHelper.WrapAngle(wingRotation - _desiredRotation);
        float rightWingRotation = MathHelper.WrapAngle(wingRotation + _desiredRotation);

        Vector2 leftDrawPosition = wingPosition.RotatedBy(leftWingRotation);
        Vector2 rightDrawPosition = (wingPosition * -Vector2.UnitY).RotatedBy(rightWingRotation);

        if (drawingGlow)
        {
            pipeline
                .DrawSprite(
                    glowTexture,
                    leftDrawPosition + drawPosition - screenPos,
                    Color.White,
                    null,
                    leftWingRotation,
                    wingTexture.Size() * 0.5f,
                    new Vector2(NPC.scale))
                .DrawSprite(
                    glowTexture,
                    rightDrawPosition + drawPosition - screenPos,
                    Color.White,
                    null,
                    rightWingRotation,
                    wingTexture.Size() * 0.5f,
                    new Vector2(NPC.scale),
                    SpriteEffects.FlipVertically);
        }
        else
        {
            pipeline
                .DrawSprite(
                    wingTexture,
                    leftDrawPosition + drawPosition - screenPos,
                    drawColor,
                    null,
                    leftWingRotation,
                    wingTexture.Size() * 0.5f,
                    new Vector2(NPC.scale))
                .DrawSprite(
                    wingTexture,
                    rightDrawPosition + drawPosition - screenPos,
                    drawColor,
                    null,
                    rightWingRotation,
                    wingTexture.Size() * 0.5f,
                    new Vector2(NPC.scale),
                    SpriteEffects.FlipVertically);
        }
    }
}