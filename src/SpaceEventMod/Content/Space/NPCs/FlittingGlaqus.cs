using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    }

    public override void AI()
    {
        NPC.TargetClosest();

        for (int i = 0; i < _body.Count; i++)
        {
            var point = _body[i];
            point.Acceleration += (Vector2.UnitY * 20f) / ((i + 1) * (i + 1));
            _body[i] = point;
        }

        _body.AnchorStart = NPC.Center;
        _body.Update(16, 0.1f, true);

        

        NPC.ai[1] = MathHelper.Lerp(NPC.ai[1], NPC.ai[0] == 0f ? 1f : 50f, 0.67f);
        
        if (!NPC.HasValidTarget)
        {
            NPC.ai[0] = 0;
            return;
        }

        _target = Main.player[NPC.target].Center;

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

        // draw wings
        var wingTexture = Assets.Textures.Space.NPCs.FlittingGlaqusWing.Asset.Value;

        int wingSegmentOrigin = 4;
        var wingRotationTotal = (_body[wingSegmentOrigin].Position - _body[wingSegmentOrigin + 1].Position).ToRotation();
        wingRotationTotal -= MathHelper.PiOver2;

        Vector2 wingOrigin = new Vector2(55, 9);

        Vector2 wingPosition = _body[wingSegmentOrigin].Position;

        var lerpValue = (MathF.Sin(1 * Main.GlobalTimeWrappedHourly) * 0.5f) + 0.5f;
        int wingFrame = (int)Math.Round(MathHelper.Lerp(0, 7, Main.GlobalTimeWrappedHourly * 0.5f % 1f));

        var wingRect = wingTexture.Frame(1, 8, 0, wingFrame);

        var color = drawColor;

        spriteBatch.Draw(wingTexture, wingPosition - screenPos, wingRect, color, wingRotationTotal, wingOrigin, NPC.scale, 0, 0);

        wingOrigin.X = wingTexture.Width - wingOrigin.X;

        spriteBatch.Draw(wingTexture, wingPosition - screenPos, wingRect, color, wingRotationTotal, wingOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);

        // draw body
        var bodyTexture = TextureAssets.Npc[Type].Value;
        var trailPoints = new List<Vector2>(_body.Positions.Length + 1);

        ReadOnlySpan<Vector2> controlPoints = _body.Positions;
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(_body.Positions.Length + 1);

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
        var LRWEOWPINM = MathHelper.Clamp(NPC.velocity.Length() / 5f, 0, 1);
        origin = Vector2.Lerp(new Vector2(19, 23), frame.Size() * 0.5f, LRWEOWPINM);

        spriteBatch.Draw(headTexture, _body[0].Position - screenPos, frame, color, targetRotation, origin, NPC.scale, 0, 0);


        return false;
    }
}
