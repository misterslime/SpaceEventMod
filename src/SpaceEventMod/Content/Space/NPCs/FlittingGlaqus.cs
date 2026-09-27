using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Geometry;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Passes;
using SpaceEventMod.Common.SDFs;
using SpaceEventMod.Common.Splines;
using SpaceEventMod.Content.Space.LevelElements;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.NPCs;

internal class FlittingGlaqus : ModNPC
{
    private VerletString _body;
    private Vector2 _target;

    public override void SetDefaults()
    {
        NPC.width = 30;
        NPC.height = 30;
        NPC.damage = 0;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 1f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.netUpdate = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        int segments = 20;

        _body = new VerletString(NPC.Center, segments, 96 / segments, MathHelper.PiOver2);
        _body.Lock(0); // replace locking with bias?
        _body.Gravity = Vector2.Zero;

        NPC.ai[0] = 0;
    }

    public override void AI()
    {
        NPC.TargetClosest();


        _body.AnchorStart = NPC.Center;

        for (int i = 1; i < _body.Count; i++)
        {
            var point = _body[i];

            var wiggleTime = NPC.ai[0];

            var gravity = (Vector2.UnitY * 20f) / ((i + 1) * (i + 1));

            point.Acceleration += gravity;
            _body[i] = point;
        }

        _body.Update(8, 0.1f, true);

        

        NPC.ai[1] = MathHelper.Lerp(NPC.ai[1], NPC.ai[0] == 0f ? 1f : 50f, 0.67f);
        NPC.ai[0]++;
        if (!NPC.HasValidTarget)
        {
            return;
        }

        _target = Main.player[NPC.target].Center;

        NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.05f);

        // we're trying to make the velocity go up and down in a wave of form ae^sin(bx)
        // where b is frequency and a is amplitude
        // first derivative and acceleration of that is abcos(bx)e^sin(bx)

        float frequency = 0.01f;
        float amplitude = 0.35f;
        float acceleration = MathF.Exp(MathF.Sin(NPC.ai[0] * frequency));
        acceleration *= frequency * amplitude * MathF.Cos(frequency * NPC.ai[0]);

        NPC.velocity -= acceleration * Vector2.UnitY;

        return;

        var toTarget = _target - (NPC.Center + NPC.velocity);

        if (toTarget.LengthSquared() > 16 * 16 * 8 * 8)
        {
            NPC.velocity += toTarget.SafeNormalize(Vector2.Zero);
            NPC.velocity = NPC.velocity.Clamp(10f);
            NPC.ai[0] = 1f;
        }
        else
        {
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.05f);
            NPC.ai[0] = 0f;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (_body is null)
            return true;

        var color = drawColor;

        // get body points
        var bodyPoints = _body.Positions.ToArray();

        for (int i = 1; i < bodyPoints.Length; i++)
        {
            var point = bodyPoints[i];

            var wiggleVector = (point - bodyPoints[i - 1]).SafeNormalize(Vector2.Zero).PerpendicularClockwise();
            var wiggleMult = MathF.Sin(((float)i / (float)bodyPoints.Length) * MathF.PI);

            wiggleVector *= MathF.Sin(i * MathF.PI / 5 + NPC.ai[0] * 0.03f) * 6 * wiggleMult * ((float)i / (float)bodyPoints.Length);

            bodyPoints[i] += wiggleVector;
        }

        var trailPoints = new List<Vector2>(bodyPoints.Length + 1);

        ReadOnlySpan<Vector2> controlPoints = bodyPoints;
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(bodyPoints.Length + 1);

        // draw wings
        var wingTexture = Assets.Textures.Space.NPCs.FlittingGlaqusWing.Asset.Value;

        int wingSegmentOrigin = 3;
        var wingRotationTotal = (_body[wingSegmentOrigin].Position - _body[wingSegmentOrigin + 1].Position).ToRotation();
        wingRotationTotal += MathHelper.PiOver2;

        Vector2 wingOrigin = new Vector2(55, 9);

        Vector2 wingPosition = trailPoints[wingSegmentOrigin + 1];

        int wingFrame = (int)Math.Floor(NPC.ai[0] * 0.12f) % 8;

        var wingRect = wingTexture.Frame(1, 8, 0, wingFrame);

        spriteBatch.Draw(wingTexture, wingPosition - screenPos, wingRect, color, wingRotationTotal, wingOrigin, NPC.scale, 0, 0);

        wingOrigin.X = wingTexture.Width - wingOrigin.X;

        spriteBatch.Draw(wingTexture, wingPosition - screenPos, wingRect, color, wingRotationTotal, wingOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);

        // draw body
        var bodyTexture = TextureAssets.Npc[Type].Value;

        Graphics.BeginPipeline()
            .DrawTrail(
                trailPoints.ToArray(),
                _ => bodyTexture.Size().Y * NPC.scale,
                _ => color,
                Assets.Shaders.Trail.BendyTexture.Asset.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("sampleTexture", bodyTexture),
                ("frame", new Vector4(0, 0, 1, 1)))
            .Flush();

        // draw head
        var headTexture = Assets.Textures.Space.NPCs.FlittingGlaqusHead.Asset.Value;

        var lookingDirection = Vector2.Zero;

        var targetPos = _target;

        var toTarget = (targetPos - NPC.Center) / 16;
        toTarget = toTarget.Clamp(30f);

        lookingDirection = toTarget;

        var length = lookingDirection.Length();
        int frameY = (int)Math.Round(MathHelper.Lerp(3, 0, MathF.Abs(length) / 30f) * MathF.Sign(lookingDirection.X));

        lookingDirection.Y *= lookingDirection.X > 0 ? 1 : -1;
        lookingDirection.X = MathF.Abs(lookingDirection.X);

        var targetRotation = 0f.AngleLerp(lookingDirection.ToRotation(), 0.3f * Math.Abs(frameY));

        var frame = headTexture.Frame(1, 13, 0, 6 + frameY);

        var origin = new Vector2(19, 23);

        spriteBatch.Draw(headTexture, _body[0].Position - screenPos, frame, color, targetRotation, origin, NPC.scale, 0, 0);


        return false;
    }

    private Vector2[] WiggleBody(Vector2[] trailPoints, float timer, float wiggleStrength, float sineLimit)
    {
        var newTrailPoints = trailPoints;

        for (var i = 0; i < trailPoints.Length; i++)
        {
            if (i < trailPoints.Length - 1)
            {
                var point = trailPoints[i];
                var nextPoint = trailPoints[i + 1];

                var displacement = point - nextPoint;
                displacement = new Vector2(-displacement.Y, displacement.X).SafeNormalize(Vector2.Zero);

                newTrailPoints[i] = point + displacement * wiggleStrength *  MathF.Sin(((sineLimit * i) / trailPoints.Length) + timer);
            }
            else
            {
                var point = trailPoints[i];
                var previousPoint = trailPoints[i - 1];

                var displacement = previousPoint - point;
                displacement = new Vector2(-displacement.Y, displacement.X).SafeNormalize(Vector2.Zero);

                newTrailPoints[i] = point + displacement * wiggleStrength * MathF.Sin(((sineLimit * i) / trailPoints.Length) + timer);
            }
        }

        return newTrailPoints;
    }
}
