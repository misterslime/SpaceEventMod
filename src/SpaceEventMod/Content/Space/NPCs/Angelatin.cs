using Daybreak.Common.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Tweening;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static tModPorter.ProgressUpdate;

namespace SpaceEventMod.Content.Space.NPCs;

internal class Angelatin : ModNPC
{
    private const int TENTACLES = 2;

    protected ref float Timer => ref NPC.ai[1];

    private Vector2 _spring = Vector2.Zero;
    
    private int _headVariant = 0;
    private int _dressVariant = 0;
    private int[] _starVariants = [];
    private int[] _segments = [];
    private Tentacle[] _tentacles = [];

    public override void SetDefaults()
    {
        NPC.width = 56;
        NPC.height = 40;
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

        _headVariant = Main.rand.Next(0, 4);
        _dressVariant = Main.rand.Next(0, 4);

        _starVariants = new int[TENTACLES];
        _tentacles = new Tentacle[TENTACLES];
        _segments = new int[TENTACLES];

        for (int i = 0; i < TENTACLES; i++)
        {
            _starVariants[i] = Main.rand.Next(0, 4);
            _segments[i] = Main.rand.Next(4, 14);
        }
    }

    public override void OnSpawn(IEntitySource source)
    {

        for (int i = 0; i < TENTACLES; i++)
        {
            _tentacles[i] = new Tentacle(NPC.Center, _segments[i], 10, MathHelper.PiOver2);

            var segment = _tentacles[i][0];
            segment.Locked = true;
            _tentacles[i][0] = segment;

            _tentacles[i].Gravity = Vector2.UnitY * 0.8f;
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_headVariant);
        writer.Write(_dressVariant);

        for (int i = 0; i < TENTACLES; i++)
        {
            writer.Write(_starVariants[i]);
            writer.Write(_segments[i]);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _headVariant = reader.ReadInt32();
        _dressVariant = reader.ReadInt32();

        for (int i = 0; i < TENTACLES; i++)
        {
            _starVariants[i] = reader.ReadInt32();
            _segments[i] = reader.ReadInt32();
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


            NPC.velocity += player.velocity;
            NPC.velocity.X -= MathHelper.Clamp(toPlayer.X, -3, 3);


            player.velocity.Y *= -1;
            player.velocity.Y -= 5;
            player.velocity.Y = player.velocity.Y > -10 ? -10 : player.velocity.Y;

            _spring.Y += -0.25f;

            SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
        }

        NPC.velocity *= 0.96f;

        var dressDimensions = new Vector2(40, 14);

        int tentacles = 2;
        float baseLineLength = 32f;
        float wavelength = 2f * MathF.PI / 5f;
        float speed = 0.015f * MathF.PI;
        float amplitude = 0.15f;

        Vector2 baseLineStart = new Vector2(baseLineLength * -0.5f, dressDimensions.Y * 0.75f);
        NPC.rotation = -NPC.velocity.X * 0.05f ;

        for (int i = 0; i < TENTACLES; i++)
        {


            _tentacles[i].AnchorStart = NPC.Center + (baseLineStart + Vector2.UnitX * ((1f + (float)i) / ((float)tentacles + 1f)) * baseLineLength).RotatedBy(-NPC.rotation * 0.5f);
            WaveMotion(_tentacles[i], i, wavelength, speed, amplitude, NPC.velocity);
            _tentacles[i].Update(8, 0.05f);
        }


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

        // transform matrix

        var drawPosition = NPC.Hitbox.Bottom();

        float springScale = 0.5f;
        var wobbleSine = MathF.Sin(Timer * MathF.PI * 0.02f) * 0.04f * (1f - _spring.X);
        var shearStrength = NPC.velocity.X * _spring.X * springScale;

        // copied slr code
        Main.instance.TilesRenderer.Wind.GetWindTime((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16, 20, out int windTimeLeft, out int directionX, out int directionY);
        Vector2 wind = new Vector2(directionX, directionY) * (windTimeLeft / 20f) * 2f;
        wind.X += (Main.windSpeedCurrent + MathF.Sin(Main.windCounter * 0.12f + NPC.Center.X * 0.01f) * Main.windSpeedCurrent * 0.3f) * (Math.Abs(NPC.Center.Y) / 180f);

        var translate = Matrix.CreateTranslation(drawPosition.X - Main.screenPosition.X, drawPosition.Y - Main.screenPosition.Y, 0f);
        var angelatinTransform = Matrix.Identity with { M12 = 0.4f * shearStrength, M21 = 0.08f * shearStrength }
             * Matrix.CreateScale(1f - _spring.X * springScale + wobbleSine, 1f + _spring.X * springScale - wobbleSine, 1f);
        
        var rotation = Matrix.CreateRotationZ(0.02f * (-wind.X / MathHelper.TwoPi) % MathHelper.TwoPi);

        using var _ = spriteBatch.Scope();
        spriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            effect: null,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: DepthStencilState.None,
            rasterizerState: RasterizerState.CullCounterClockwise,
            transformMatrix: angelatinTransform * rotation * translate * Main.GameViewMatrix.TransformationMatrix);

        var headTexture = TextureAssets.Npc[Type].Value;
        var dressTexture = Assets.Textures.Space.NPCs.AngelatinDress.Asset.Value;
        var starTexture = Assets.Textures.Space.NPCs.AngelatinStrandStars.Asset.Value;
        var strandLeftTexture = Assets.Textures.Space.NPCs.AngelatinStrandLeft.Asset.Value;
        var strandRightTexture = Assets.Textures.Space.NPCs.AngelatinStrandLeft.Asset.Value;

        var headDimensions = new Vector2(56, 40);
        var dressDimensions = new Vector2(40, 14);
        var segmentDimensions = new Vector2(4, 10);
        var starDimensions = new Vector2(14, 12);

        float baseLineLength = 16f;
        var color = new Color(Lighting.GetSubLight(NPC.Center));

        Vector2 baseLineStart = NPC.Hitbox.Bottom() - Vector2.Transform(new Vector2(0f, dressTexture.Size().Y + 4f), angelatinTransform);

        int variant = 0;

        for (int i = 0; i < TENTACLES; i++)
        {
            if (_tentacles[i] is null)
                continue;

            DrawTentacle(spriteBatch, _tentacles[i], i, true, baseLineStart, color, ref variant);
        }

        var frame = dressTexture.Frame(4, 1, _dressVariant, 0);
        var origin = frame.Size() * new Vector2(0.5f, 1f);

        spriteBatch.Draw(dressTexture, Vector2.Zero, frame, color, -NPC.rotation * 0.66f, origin with { Y = 0 }, 1f, 0, 0);

        frame = headTexture.Frame(4, 1, _headVariant, 0);
        origin = frame.Size() * new Vector2(0.5f, 1f);

        spriteBatch.Draw(headTexture, Vector2.Zero, frame, color, NPC.rotation, origin, 1f, 0, 0);

        spriteBatch.End();

        return false;
    }

    private void DrawTentacle(SpriteBatch spriteBatch, Tentacle tentacle, int tentacleIndex, bool left, Vector2 displace, Color lightColor, ref int segmentVariant)
    {
        var pixel = Assets.Textures.WhitePixel.Asset.Value;
        var starTexture = Assets.Textures.Space.NPCs.AngelatinStrandStars.Asset.Value;
        var strandLeftTexture = Assets.Textures.Space.NPCs.AngelatinStrandLeft.Asset.Value;
        var strandRightTexture = Assets.Textures.Space.NPCs.AngelatinStrandLeft.Asset.Value;


        for (int i = 0; i < tentacle.Count; i++)
        {
            var currentPosition = tentacle[i].Position;

            if (i == tentacle.Count - 1)
            {
                var previousPosition = tentacle[i - 1].Position;

                float starAngle = tentacle[i].Position.DirectionFrom(previousPosition).ToRotation();
                var starFrame = starTexture.Frame(4, 1, _starVariants[tentacleIndex], 0);


                spriteBatch.Draw(starTexture, currentPosition - displace, starFrame, lightColor, starAngle - MathHelper.PiOver2, starFrame.Size() * 0.5f, 1f, 0, 0);

                continue;
            }

            var nextPosition = tentacle[i + 1].Position;

            float angle = nextPosition.DirectionTo(currentPosition).ToRotation();


            var segmentFrame = strandLeftTexture.Frame(3, 1, segmentVariant % 3, 0);

            // texture scaling
            Vector2 scale = new Vector2(1f, currentPosition.Distance(nextPosition) / segmentFrame.Height);
            Vector2 segmentOrigin = segmentFrame.Size() * new Vector2(0.5f, 0f);

            spriteBatch.Draw(left ? strandLeftTexture : strandRightTexture, nextPosition - displace, segmentFrame, lightColor, angle - MathHelper.PiOver2, segmentOrigin, scale, 0, 0);
        }
    }

    private void WaveMotion(Tentacle tentacle, int tentacleIndex, float wavelength, float speed, float amplitude, Vector2 tentacleVel)
    {
        // tentacle movement displacement
        float animDisplacement = (2f * ((float)tentacleIndex / (float)(TENTACLES - 1f)) - 1f);

        // progress and start angle
        float tentProgress = ((float)tentacleIndex / (float)TENTACLES);

        // wave effect
        for (int i = 0; i < tentacle.Count - 1; i++)
        {
            var next = tentacle[i + 1];

            float progress = EasingFunctions.InCubic(i / (float)tentacle.Count);

            // tentacle sine wave
            float transverse = (2f / wavelength) * (MathF.PI * progress - Timer * speed) - tentProgress * MathF.PI * 0.5f;
            float newRot = Math.Clamp(tentacleVel.Y * 0.015f, -1f, 1f) * animDisplacement * (1f - progress);

            float angularAcceleration = animDisplacement * amplitude * (wavelength * MathF.Sin(transverse) + 2 * MathF.PI * progress * MathF.Cos(transverse));
            angularAcceleration /= wavelength;
            angularAcceleration += newRot;

            // add up angles
            // angle += wave * animDisplacement * amplitude;

            var angle = (next.Position - tentacle[i].Position).PerpendicularClockwise().SafeNormalize(Vector2.Zero);

            var linearAcceleration = angularAcceleration * angle;

            next.Acceleration -= linearAcceleration;

            tentacle[i + 1] = next;
        }

        // wind

        for (int i = 0; i < tentacle.Count; i++)
        {
            var current = tentacle[i];
            float progress = i / (float)tentacle.Count;

            float res = EasingFunctions.InSine(progress);

            // copied slr code
            Vector2 lerped = Vector2.Lerp(NPC.Hitbox.Bottom(), tentacle.AnchorStart, progress);
            Main.instance.TilesRenderer.Wind.GetWindTime((int)current.Position.X / 16, (int)current.Position.Y / 16, 20, out int windTimeLeft, out int directionX, out int directionY);
            Vector2 wind = new Vector2(directionX, directionY) * (windTimeLeft / 20f) * 2f;
            wind.X += (Main.windSpeedCurrent + MathF.Sin(Main.windCounter * 0.12f + current.Position.X * 0.01f) * Main.windSpeedCurrent * 0.8f) * (Math.Abs(current.Position.Y - lerped.Y) / 180f);

            current.Acceleration += wind * res;

            tentacle[i] = current;
        }
    }
}
