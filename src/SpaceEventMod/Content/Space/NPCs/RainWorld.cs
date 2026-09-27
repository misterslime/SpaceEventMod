using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Physics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.NPCs;

internal class RainWorld : ModNPC
{
    private VerletString _body;

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
        NPC.noTileCollide = true;
        NPC.netUpdate = true;

        int segments = 20;

        _body = new VerletString(NPC.Center, segments, 96 / segments, MathHelper.PiOver2);
        _body.Lock(0); // replace locking with bias?
        _body.Gravity = Vector2.UnitY * 2f;
    }

    public override void AI()
    {
        NPC.TargetClosest();

        NPC.Center = Main.MouseWorld - Vector2.UnitY * 16 * 10;

        _body.AnchorStart = NPC.Center;
        _body.Update(8, 0.15f, true);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (_body is null)
            return true;

        // draw wings
        var largeWingTexture = Assets.Textures.Space.NPCs.RainWorldLargeWing.Asset.Value;
        var smallWingTexture = Assets.Textures.Space.NPCs.RainWorldSmallWing.Asset.Value;

        int wingSegmentOrigin = 4;
        var wingRotationTotal = (_body[wingSegmentOrigin].Position - _body[wingSegmentOrigin + 1].Position).ToRotation();
        wingRotationTotal -= MathHelper.PiOver2;

        Vector2 largeWingOrigin = new Vector2(51, 4);
        Vector2 smallWingOrigin = new Vector2(35, 20);

        Vector2 wingPosition = _body[wingSegmentOrigin].Position;

        var wingSpeed = 1f;
        var lerpValue = (MathF.Sin(wingSpeed * Main.GlobalTimeWrappedHourly) * 0.5f) + 0.5f;
        var wingRotation = MathHelper.Lerp(-MathHelper.PiOver4, MathHelper.PiOver4, lerpValue);

        var lerpValue2 = (MathF.Sin(2 * wingSpeed * Main.GlobalTimeWrappedHourly) * 0.5f) + 0.5f;
        var color = Color.White;

        spriteBatch.Draw(largeWingTexture, wingPosition - screenPos, null, color, wingRotationTotal - wingRotation, largeWingOrigin, NPC.scale, 0, 0);
        spriteBatch.Draw(smallWingTexture, wingPosition - screenPos, null, color, wingRotationTotal + wingRotation, smallWingOrigin, NPC.scale, 0, 0);

        largeWingOrigin.X = largeWingTexture.Width - largeWingOrigin.X;
        smallWingOrigin.X = smallWingTexture.Width - smallWingOrigin.X;

        spriteBatch.Draw(largeWingTexture, wingPosition - screenPos, null, color, wingRotationTotal + wingRotation, largeWingOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);
        spriteBatch.Draw(smallWingTexture, wingPosition - screenPos, null, color, wingRotationTotal - wingRotation, smallWingOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0);

        // draw body
        var bodyTexture = TextureAssets.Npc[Type].Value;

        Graphics.BeginPipeline()
            .DrawTrail(
                _body.Positions,
                _ => bodyTexture.Size().Y * NPC.scale,
                _ => color,
                Assets.Shaders.Trail.BendyTexture.Asset.Value,
                ("transformMatrix", Graphics.WorldTransformMatrix),
                ("sampleTexture", bodyTexture),
                ("frame", new Vector4(0, 0, 1, 1)))
            .Flush();

        // draw head
        var headTexture = Assets.Textures.Space.NPCs.RainWorldHead.Asset.Value;

        var lookingDirection = Vector2.Zero;

        if (NPC.HasValidTarget)
        {
            var targetPos = Main.player[NPC.target].Center;

            var toTarget = (targetPos - NPC.Center) / 16;
            toTarget = toTarget.Clamp(30f);

            lookingDirection = toTarget;
        }

        var length = lookingDirection.Length();
        int frameY = (int)Math.Round(MathHelper.Lerp(3, 0, MathF.Abs(length) / 30f) * MathF.Sign(lookingDirection.X));

        lookingDirection.Y *= lookingDirection.X > 0 ? 1 : -1;
        lookingDirection.X = MathF.Abs(lookingDirection.X);

        var targetRotation = 0f.AngleLerp(lookingDirection.ToRotation(), 0.3f * Math.Abs(frameY));


        var frame = headTexture.Frame(1, 13, 0, 6 + frameY);

        var origin = frame.Size() * 0.5f;
        origin = new Vector2(19, 23);

        spriteBatch.Draw(headTexture, _body[0].Position - screenPos, frame, color, targetRotation, origin, NPC.scale, 0, 0);


        return false;
    }
}
