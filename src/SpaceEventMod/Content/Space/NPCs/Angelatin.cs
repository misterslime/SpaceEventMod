using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.NPCs;

internal class Angelatin : ModNPC
{
    private const int TENTACLES = 2;

    protected ref float Timer => ref NPC.ai[1];

    private Vector2 _spring = Vector2.Zero;
    
    private int _headVariant = 0;
    private int _dressVariant = 0;
    private int[] _starVariants = [];
    private int[] _segmentNumber = [];

    public override void SetDefaults()
    {
        NPC.width = 56;
        NPC.height = 40;
        NPC.damage = 0;
        NPC.defense = 16;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = -1;

        NPC.noGravity = true;
        NPC.noTileCollide = true;

        _headVariant = Main.rand.Next(0, 4);
        _dressVariant = Main.rand.Next(0, 4);

        _starVariants = new int[TENTACLES];
        _segmentNumber = new int[TENTACLES];

        for (int i = 0; i < TENTACLES; i++)
        {
            _starVariants[i] = Main.rand.Next(0, 4);
            _segmentNumber[i] = Main.rand.Next(4, 9);
        }
    }

    public override void AI()
    {
        Timer++;

        float dampening = 0.045f;
        float tension = 0.08f;

        var acceleration = -tension * _spring.X - dampening * _spring.Y;

        _spring.Y += acceleration;
        _spring.X += _spring.Y;

        foreach (var player in Main.ActivePlayers)
        {
            if (!player.Hitbox.Intersects(NPC.Hitbox) ||
                player.velocity.Y <= 0)
                continue;

            var toPlayer = player.Center - NPC.Center;


            NPC.velocity += player.velocity * 0.5f;
            NPC.velocity.X -= MathHelper.Clamp(toPlayer.X, -3, 3);


            player.velocity.Y *= -1;
            player.velocity.Y -= 5;
            player.velocity.Y = player.velocity.Y > -10 ? -10 : player.velocity.Y;

            _spring.Y += -0.25f;

            SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
        }

        NPC.velocity *= 0.96f;

        return; // remove this line to make them hate you

        NPC.TargetClosest();
        NPC.damage = 67;

        if (!NPC.HasValidTarget)
            return;

        var toTarget = (Main.player[NPC.target].Center - (NPC.Center + NPC.velocity)).SafeNormalize(Vector2.Zero);
        NPC.velocity += toTarget;
        NPC.velocity = NPC.velocity.Clamp(10f);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        using var _ = spriteBatch.Scope();

        // transform matrix

        var drawPosition = NPC.Hitbox.Bottom();

        float springScale = 0.5f;
        var wobbleSine = MathF.Sin(Timer * MathF.PI * 0.02f) * 0.04f * (1f - _spring.X);
        var shearStrength = NPC.velocity.X * _spring.X * springScale;

        var angelatinTransform = Matrix.Identity with { M12 = 0.4f * shearStrength, M21 = 0.08f * shearStrength }
             * Matrix.CreateScale(1f - _spring.X * springScale + wobbleSine, 1f + _spring.X * springScale - wobbleSine, 1f)
             * Matrix.CreateTranslation(drawPosition.X - Main.screenPosition.X, drawPosition.Y - Main.screenPosition.Y, 0f);

        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            effect: null,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.None,
            rasterizerState: RasterizerState.CullCounterClockwise,
            transformMatrix: angelatinTransform * Main.GameViewMatrix.TransformationMatrix);

        var headTexture = TextureAssets.Npc[Type].Value;
        var dressTexture = Assets.Textures.Space.NPCs.AngelatinDress.Asset.Value;
        var starTexture = Assets.Textures.Space.NPCs.AngelatinStrandStars.Asset.Value;
        var strandLeftTexture = Assets.Textures.Space.NPCs.AngelatinStrandLeft.Asset.Value;
        var strandRightTexture = Assets.Textures.Space.NPCs.AngelatinStrandLeft.Asset.Value;

        var headDimensions = new Vector2(56, 40);
        var dressDimensions = new Vector2(40, 14);
        var segmentDimensions = new Vector2(4, 10);
        var starDimensions = new Vector2(14, 12);

        int segmentLength = 10;
        float wavelength = 4f * MathF.PI / 5f;
        float speed = 0.015f * MathF.PI;
        float amplitude = 0.4f;
        float baseLineLength = 16f;
        var color = new Color(Lighting.GetSubLight(NPC.Center));

        Vector2 baseLineStart = new Vector2(baseLineLength * -0.5f, dressDimensions.Y);
        var rotation = -NPC.velocity.X * 0.05f;

        int segmentVariant = 0;

        for (int tentacle = 0; tentacle < TENTACLES; tentacle++)
        {
            // tentacle movement displacement
            float tentaclePolynomial = (2f * ((float)tentacle / (float)(TENTACLES - 1f)) - 1f);

            // tentacle position
            Vector2 position = baseLineStart + Vector2.UnitX * ((1f + (float)tentacle) / ((float)TENTACLES + 1f)) * baseLineLength;
            position = position.RotatedBy(-rotation * 0.5f);
            //position += _position;

            // progress and start angle
            float tentProgress = ((float)tentacle / (float)TENTACLES);
            float angle = MathHelper.PiOver2 + -rotation * 0.5f;

            for (int i = 0; i < _segmentNumber[tentacle]; i++)
            {
                float progress = i / (float)_segmentNumber[tentacle];

                // tentacle sine wave
                float segmentWave = (2f / wavelength) * (MathF.PI * progress - Timer * speed);
                float wave = MathF.Sin(segmentWave + tentProgress * MathF.PI * 0.5f) * progress;

                // add up angles
                angle += wave * tentaclePolynomial * amplitude;

                //Vector2 end = position + Vector2.UnitY * segmentLength + Vector2.UnitX * wave * tentaclePolynomial * amplitude;

                Vector2 end = position + angle.ToRotationVector2() * segmentLength;

                var segmentFrame = strandLeftTexture.Frame(3, 1, segmentVariant % 3, 0);
                //angle = position.AngleTo(end);

                // texture scaling
                Vector2 scale = new Vector2(1, position.Distance(end) / segmentFrame.Height);
                Vector2 segmentOrigin = segmentFrame.Size() * new Vector2(0.5f, 0f);

                if (tentacle + 1 <= TENTACLES * 0.5f)
                    spriteBatch.Draw(strandLeftTexture, end, segmentFrame, color, angle + MathHelper.PiOver2, segmentOrigin, 1f, 0, 0);
                else
                    spriteBatch.Draw(strandRightTexture, end, segmentFrame, color, angle + MathHelper.PiOver2, segmentOrigin, 1f, 0, 0);

                position = end;

                segmentVariant++;

                if (i != _segmentNumber[tentacle] - 1)
                    continue;

                var starFrame = starTexture.Frame(4, 1, _starVariants[tentacle], 0);

                spriteBatch.Draw(starTexture, position, starFrame, color, angle - MathHelper.PiOver2, starFrame.Size() * 0.5f, 1f, 0, 0);
            }
        }

        var frame = dressTexture.Frame(4, 1, _dressVariant, 0);
        var origin = frame.Size() * new Vector2(0.5f, 1f);

        spriteBatch.Draw(dressTexture, Vector2.Zero, frame, color, -rotation * 0.66f, origin with { Y = 0 }, 1f, 0, 0);

        frame = headTexture.Frame(4, 1, _headVariant, 0);
        origin = frame.Size() * new Vector2(0.5f, 1f);

        spriteBatch.Draw(headTexture, Vector2.Zero, frame, color, rotation, origin, 1f, 0, 0);

        spriteBatch.End();

        return false;
    }
}
