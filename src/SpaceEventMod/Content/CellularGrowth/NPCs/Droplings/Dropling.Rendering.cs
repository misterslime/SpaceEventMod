using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation.Splines;
using SpaceEventMod.Core.Animation.Tweening;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Components;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace SpaceEventMod.Content.CellularGrowth.NPCs.Droplings;

internal partial class Dropling
{
    private float _wiggleTimer = 0;

    // duration in seconds here but can be anything
    private readonly static EasingMotion s_droplingHeartbeat = new EasingMotion()
        .ChainMotion(duration: 0.15f, endValue: 1f, Ease.InOutQuint)
        .ChainMotion(duration: 0.15f, endValue: 0.2f, Ease.InCirc)
        .DelayMotion(duration: 0.1f)
        .ChainMotion(duration: 0.15f, endValue: 0.75f, Ease.InSine)
        .ChainMotion(duration: 0.15f, endValue: 0, Ease.InCirc)
        .DelayMotion(duration: 0.65f);

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

        headPosition *= NPC.width * 0.5f * NPC.scale;
        headPosition = drawPosition + headPosition;

        tailPosition *= NPC.width * 0.5f * NPC.scale;
        tailPosition = drawPosition - tailPosition;

        ReadOnlySpan<Vector2> controlPoints = new Vector2[] { headPosition, drawPosition, tailPosition };
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(segments + 1);

        var wiggleStrength = Math.Clamp(NPC.velocity.Length() * 0.5f, 1, 4);
        var sineLimit = (3.5f * MathF.PI) / 2;

        _wiggleTimer += 1 / 8f + wiggleStrength / 30f;

        Vector2[] trailPointsArray = WiggleBody(in trailPoints, _wiggleTimer, wiggleStrength, sineLimit, bendiness);

        Pipeline pipeline = Graphics.BeginPipeline();

        if (HasAppendage(DroplingAppendage.Wings))
        {
            DrawWings(in pipeline, in trailPointsArray, trailPoints[7], screenPos, drawColor);
        }

        if (HasAppendage(DroplingAppendage.Flagellum))
        {
            _flagellum.Center = new(trailPoints[16]);

            DrawTail(in pipeline, _flagellumTail1, 0, Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingTentacle2.Asset.Value, screenPos, drawColor);
            DrawTail(in pipeline, _flagellumTail2, 1, Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingTentacle2.Asset.Value, screenPos, drawColor);
            DrawTail(in pipeline, _flagellumTail3, 2, Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingTentacle2.Asset.Value, screenPos, drawColor);
        }

        pipeline
            .DrawTrail(
                trailPointsArray,
                _ => NPC.height * NPC.scale,
                _ => drawColor,
                Assets.Shaders.Trail.BendyTexture.Asset.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("sampleTexture", texture),
                ("frame", new Vector4(0, (float)AppendageFrame(), 1, 4)));

        if (HasAppendage(DroplingAppendage.Wings))
        {
            DrawWings(in pipeline, in trailPointsArray, trailPoints[7], screenPos, drawColor, true);
        }

        pipeline.Schedule(RenderLayer.AfterNPCs);

        Texture2D starTexture = Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingStar.Asset.Value;
        Texture2D starGlowTexture = Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingStar_Glow.Asset.Value;

        Vector2 starPositionDifferenceFromCenter = trailPoints[7] - drawPosition;
        starPositionDifferenceFromCenter *= 0.25f;

        Vector2 starPosition = drawPosition + starPositionDifferenceFromCenter + new Vector2(4, 0).RotatedBy(NPC.rotation) * NPC.scale;

        float time = (NPC.whoAmI * 0.13f + Main.GlobalTimeWrappedHourly);
        float heartbeat = s_droplingHeartbeat.Evaluate(time, out bool completed);

        float starScale = 0.75f + 0.75f * heartbeat;
        float starRotation = (trailPoints[7] - trailPoints[8]).ToRotation();

        Color starColor = Color.Cyan;
        starColor.A = 0;

        spriteBatch.Draw(starGlowTexture, starPosition - screenPos, null, starColor, starRotation, starGlowTexture.Size() * 0.5f, NPC.scale * starScale * 0.95f, 0, 0);
        spriteBatch.Draw(starTexture, starPosition - screenPos, null, Color.White, starRotation, starTexture.Size() * 0.5f, NPC.scale * starScale, 0, 0);

        Texture2D jawsTexture = Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingJaw.Asset.Value;
        Texture2D bigJawsTexture = Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingJawBig.Asset.Value;

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

        if (Timer <= 60f || State != DroplingState.Biting || Timer > 85f)
            return false;

        /*Fade.Draw(0.5f, (in Pipeline pipeline) =>
        {
            if (HasAppendage(DroplingAppendage.Wings))
            {
                DrawWings(in pipeline, in trailPointsArray, trailPoints[7], screenPos, drawColor);
            }

            if (HasAppendage(DroplingAppendage.Flagellum))
            {
                _flagellum.Center = new(trailPoints[16]);

                DrawTail(in pipeline, _flagellumTail1, 0, Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingTentacle2.Asset.Value, screenPos, drawColor);
                DrawTail(in pipeline, _flagellumTail2, 1, Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingTentacle2.Asset.Value, screenPos, drawColor);
                DrawTail(in pipeline, _flagellumTail3, 2, Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingTentacle2.Asset.Value, screenPos, drawColor);
            }

            pipeline
                .DrawTrail(
                    trailPointsArray,
                    _ => NPC.height * NPC.scale,
                    _ => drawColor,
                    Assets.Shaders.Trail.BendyTexture.Asset.Value,
                    ("transformMatrix", Graphics.WorldTransformMatrix),
                    ("sampleTexture", texture),
                    ("frame", new Vector4(0, (float)AppendageFrame(), 1, 4)));

            if (HasAppendage(DroplingAppendage.Wings))
            {
                DrawWings(in pipeline, in trailPointsArray, trailPoints[7], screenPos, drawColor, true);
            }

            if (HasAppendage(DroplingAppendage.BigJaw))
                pipeline.DrawSprite(bigJawsTexture, jawPosition, null, null, jawRotation, jawTextureOrigins["bigJaw"], new(NPC.scale), 0);
            else
                pipeline.DrawSprite(jawsTexture, jawPosition, null, null, jawRotation, jawTextureOrigins["jaw"], new(NPC.scale), 0);

            float the = (Timer - 60) / 25f;

            the = EasingFunctions.InQuart(Math.Clamp(the, 0.3f, 0.8f));

            pipeline.ApplyEffect(
                Assets.Shaders.Fragment.ChangeColor.Asset.Value,
                ("color", drawColor.ToVector4() * Vector4.Lerp(Color.BlueViolet.ToVector4(), Color.Blue.ToVector4(), the)));
        });*/


        return false;
    }

    private Vector2[] WiggleBody(in List<Vector2> trailPoints, float timer, float wiggleStrength, float sineLimit, float bendiness)
    {
        var newTrailPoints = trailPoints;

        for (var i = 0; i < trailPoints.Count; i++)
        {
            if (i < trailPoints.Count - 1)
            {
                var point = trailPoints[i];
                var nextPoint = trailPoints[i + 1];

                var displacement = point - nextPoint;
                displacement = new Vector2(-displacement.Y, displacement.X).SafeNormalize(Vector2.Zero);

                newTrailPoints[i] = point + displacement * wiggleStrength * (float)Math.Clamp(bendiness, 0.5, 1) * MathF.Sin(((sineLimit * i) / trailPoints.Count) + timer);
            }
            else
            {
                var point = trailPoints[i];
                var previousPoint = trailPoints[i - 1];

                var displacement = previousPoint - point;
                displacement = new Vector2(-displacement.Y, displacement.X).SafeNormalize(Vector2.Zero);

                newTrailPoints[i] = point + displacement * wiggleStrength * (float)Math.Clamp(bendiness, 0.5, 1) * MathF.Sin(((sineLimit * i) / trailPoints.Count) + timer);
            }
        }

        return newTrailPoints.ToArray();
    }

    private List<Vector2> WiggleTail(in List<Vector2> trailPoints, float timer, float wiggleStrength, float sineLimit, float bendiness)
    {
        var newTrailPoints = trailPoints;

        for (var i = 0; i < trailPoints.Count; i++)
        {
            float lerp = EasingFunctions.InOutCubic((float)i / (float)trailPoints.Count);

            float strength = MathHelper.Lerp(0, wiggleStrength, lerp) + 2f;

            if (i < trailPoints.Count - 1)
            {
                var point = newTrailPoints[i];
                var nextPoint = newTrailPoints[i + 1];

                var displacement = point - nextPoint;
                displacement = new Vector2(-displacement.Y, displacement.X).SafeNormalize(Vector2.Zero);

                point += displacement * strength * (float)Math.Clamp(bendiness, 0.5, 1) * MathF.Sin(((sineLimit * i) / trailPoints.Count) + timer);

                newTrailPoints[i] = point;
            }
        }

        return newTrailPoints;
    }

    private void DrawTail(in Pipeline pipeline, PhysicsObject physicsObject, int which, Texture2D texture, Vector2 screenPos, Color drawColor)
    {
        PhysicsShape shape = physicsObject.GetComponent<PhysicsShape>();

        var wiggleStrength = 20 * physicsObject.Center.GetVelocity(1 / 60f).Length();

        var sineLimit = (2 * MathF.PI);

        var timeDisplacement = MathHelper.TwoPi / 3;
        timeDisplacement *= which;

        //shape.Points = ApplyWiggleToPoints(shape.Points, Main.GlobalTimeWrappedHourly * -3 + timeDisplacement, wiggleStrength, sineLimit, 0f);

        List<Vector2> points = new List<Vector2>();

        points.Add(physicsObject.Center.Position);

        for (int i = 0; i < shape.Points.Length; i++)
            points.Add(shape.Points[i].Position);

        var trailPoints = new List<Vector2>();

        ReadOnlySpan<Vector2> controlPoints = points.ToArray();
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(20);

        var wiggledPoints = WiggleTail(in trailPoints, Main.GlobalTimeWrappedHourly * -8 + timeDisplacement, wiggleStrength, sineLimit, 0f);

        wiggledPoints.RemoveAt(wiggledPoints.Count - 1);

        pipeline
            .DrawTrail(
                trailPoints.ToArray(),
                _ => texture.Width,
                _ => drawColor,
                Assets.Shaders.Trail.RepeatingTexture.Asset.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("sampleTexture", texture),
                ("repeats", points.Count),
                ("spriteRotation", 1));
    }

    private void DrawWings(in Pipeline pipeline, in Vector2[] trailPoints, Vector2 drawPosition, Vector2 screenPos, Color drawColor, bool drawingGlow = false)
    {
        Texture2D wingTexture = Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingWing.Asset.Value;
        Texture2D glowTexture = Assets.Textures.CellularGrowth.NPCs.Droplings.DroplingWing_Glow.Asset.Value;

        Vector2 wingPosition = new Vector2(6, 26) * NPC.scale;

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
