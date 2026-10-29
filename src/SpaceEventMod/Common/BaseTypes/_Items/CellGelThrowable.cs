using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.Interfaces;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Splines;
using SpaceEventMod.Common.Tweening;
using SpaceEventMod.Content.Space;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.BaseTypes;

internal abstract class CellGelThrowableItem : ModItem
{
    protected abstract Vector3 LightColor { get; }
    protected abstract int ThrownProjectile { get; }
    protected abstract int Frames { get; }
    protected abstract int Damage { get; }
    protected abstract float Knockback { get; }

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, Frames));
        ItemID.Sets.AnimatesAsSoul[Type] = true;

        ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
        Item.ResearchUnlockCount = 99;
    }

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.SetShopValues(ItemRarityID.White, Item.sellPrice(copper: 35));
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.maxStack = Item.CommonMaxStack;

        Item.channel = true;
        Item.useTime = Item.useAnimation = 25;
        Item.UseSound = SoundID.Item1;
        Item.consumable = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.damage = Damage;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = Knockback;
        Item.shootSpeed = 0f;
        Item.shoot = ThrownProjectile;
    }

    public override bool CanUseItem(Player player) => player.heldProj == -1;

    public override void PostUpdate()
    {
        Lighting.AddLight(Item.Center, LightColor);
    }
}

internal abstract class CellGelThrowable : ModProjectile, IBall
{
    private enum CellGellThrowableState : Byte
    {
        TargetingCurve,
        InMotion,
        Explosion,
        Physics
    }

    private CellGellThrowableState State
    {
        get => (CellGellThrowableState)(int)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    private List<Vector2> _mousePositions = [];
    private List<Vector2> _moveToPositions = [];
    private Vector2 _oldCenter = Vector2.Zero;
    private float _pathColorMultiplier = 1f;
    private int _variant = 0;

    protected abstract Asset<Texture2D> Glow { get; }
    protected abstract Color BloomColor { get; }
    protected abstract Color GlowColor { get; }
    protected abstract Vector3 LightColor { get; }
    protected abstract int Frames { get; }
    protected abstract int Damage { get; }
    protected abstract float Knockback { get; }
    protected abstract float GlowScale { get; }

    public float Radius { get => 9f; }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;
        ProjectileID.Sets.Explosive[Type] = true;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.damage = Damage;
        Projectile.knockBack = Knockback;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;

        _mousePositions = new List<Vector2>();
        _moveToPositions = [];
        _pathColorMultiplier = 1f;
        _variant = Main.rand.Next(4);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_pathColorMultiplier);
        writer.WriteVector2(_oldCenter);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _pathColorMultiplier = reader.ReadSingle();
        _oldCenter = reader.ReadVector2();
    }

    public override void AI()
    {
        Projectile.rotation += 0.1f * _pathColorMultiplier + 0.01f;

        Lighting.AddLight(Projectile.Center, LightColor);

        if (Projectile.owner != Main.myPlayer)
            return;

        Projectile.frameCounter++;

        if (Projectile.frameCounter >= 5)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % Frames;
        }

        State = State switch
        {
            CellGellThrowableState.TargetingCurve => TargetingCurve(),
            CellGellThrowableState.InMotion => InMotion(),
            CellGellThrowableState.Explosion => Explosion(),
            CellGellThrowableState.Physics => Physics(),
            _ => CellGellThrowableState.Explosion
        };
    }

    private CellGellThrowableState TargetingCurve()
    {
        if (!Projectile.TryGetOwner(out Player? player))
        {
            //Projectile.Kill();
            return CellGellThrowableState.Explosion;
        }

        Debug.Assert(player is not null);

        if (!player.active)
        {
            Projectile.Kill();
            return CellGellThrowableState.Explosion;
        }

        Debug.Assert(player.HandPosition is not null);

        // part of this is from spirit reforged btw
        Vector2 pointTowards = _mousePositions.Count == 0 ? Main.MouseWorld : _mousePositions.First();

        int dir = pointTowards.X > player.position.X ? 1 : -1;

        player.ChangeDir(dir);
        player.heldProj = Projectile.whoAmI;
        Projectile.Center = player.HandPosition.Value - Vector2.UnitY * 8f;
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = MathHelper.WrapAngle(-dir * MathHelper.PiOver4 + MathF.Pow(MathF.Sin(Main.GlobalTimeWrappedHourly * 16f), 3) * 0.4f);
        Projectile.netUpdate = true;

        if (player.channel)
            player.itemTime = player.itemAnimation = 25;

        if (!player.channel && player.itemTime <= 17)
            return CellGellThrowableState.InMotion;

        if (_mousePositions.Count == 0 || (Main.MouseWorld - _mousePositions.Last()).LengthSquared() >= Math.Pow(64f, 2))
            _mousePositions.Add(Main.MouseWorld);

        List<Vector2> positionsToGoTo = [
            Projectile.Center, 
            Vector2.Lerp(Projectile.Center, _mousePositions.First(), 0.33f), 
            Vector2.Lerp(Projectile.Center, _mousePositions.First(), 0.67f), 
            .. _mousePositions, Main.MouseWorld];

        var segments = 50;
        var trailPoints = new List<Vector2>(segments + 1);

        using (var curve = new BezierCurve([.. positionsToGoTo]))
            trailPoints = curve.GetPoints(segments + 1);

        _moveToPositions = trailPoints;

        Projectile.timeLeft = 180;

        return CellGellThrowableState.TargetingCurve;
    }

    private CellGellThrowableState InMotion()
    {
        Projectile.netUpdate = true;

        if (MoveAlongCurve())
        {
            Projectile.velocity = Vector2.Zero;
            Physics();
            return CellGellThrowableState.Physics;
        }

        return CellGellThrowableState.InMotion;
    }

    private bool MoveAlongCurve()
    {
        Projectile.ai[1] += 0.67f;
        int current = (int)Projectile.ai[2];
        _pathColorMultiplier = MathHelper.Lerp(0f, _pathColorMultiplier, 0.95f);

        if (Projectile.ai[1] >= 1.0f)
        {
            // wrap back to 0 (or before 1)
            current++;
            Projectile.ai[1] -= 1.0f;
            Projectile.ai[2] = current;
        }

        if (current >= _moveToPositions.Count - 1)
        {
            // curve completed
            current = _moveToPositions.Count - 2;
            return true;
        }

        Projectile.timeLeft = 1600;

        int count = _moveToPositions.Count;

        Vector2 p1 = _moveToPositions[current];
        Vector2 p2 = _moveToPositions[current + 1];

        Vector2 p0 = (current == 0) ? p1 : _moveToPositions[current - 1];
        Vector2 p3 = (current == count - 2) ? p2 : _moveToPositions[current + 2];

        Vector2 newCenter = Vector2.CatmullRom(p0, p1, p2, p3, Projectile.ai[1]);
        Vector2 velocity = newCenter - Projectile.Center;

        float lookAhead = Math.Min(Projectile.ai[1] + 0.001f, 1.0f);
        Vector2 direction = Vector2.CatmullRom(p0, p1, p2, p3, lookAhead) - Projectile.Center;

        if (velocity != VerletString.TileCollision(Projectile.Center, velocity, true))
            return true;

        _oldCenter = Projectile.Center;
        Projectile.Center = newCenter;

        return false;
    }

    private CellGellThrowableState Explosion()
    {
        Projectile.PrepareBombToBlow();
        Main.NewText("srry gng explosion doesnt rlly exist yet");
        return CellGellThrowableState.Explosion;
    }

    private CellGellThrowableState Physics()
    {
        _pathColorMultiplier = MathHelper.Lerp(0f, _pathColorMultiplier, 0.95f);
        Projectile.velocity = Vector2.Zero;

        Vector2 velocity = Projectile.Center - _oldCenter;
        velocity *= 0.975f;
        //velocity += Vector2.UnitY * 0.2f;


        velocity = VerletString.TileCollision(Projectile.Center, velocity, true, Radius, -1f);
        

        _oldCenter = Projectile.Center;
        Projectile.Center = Projectile.Center + velocity;

        return CellGellThrowableState.Physics;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.timeLeft = 0;
        Projectile.PrepareBombToBlow();
        return true;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (projHitbox.Intersects(targetHitbox))
        {
            Projectile.timeLeft = 0;
            Projectile.PrepareBombToBlow();
            return true;
        }
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void PrepareBombToBlow()
    {
        Projectile.tileCollide = false;
        Projectile.alpha = 255;

        Projectile.Resize(60, 60);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D bloomTexture = Assets.Textures.Particles.Glow.Asset.Value;
        Texture2D bloomTexture2 = Assets.Textures.Particles.BloomFlare.Asset.Value;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        float colorMult = EasingFunctions.InOutSine(MathHelper.Clamp(Projectile.timeLeft / 8f, 0f, 1f));

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Texture2D glowTexture = Glow.Value;
        Color bloomColor = BloomColor;
        Color glowColor = GlowColor;
        float glowScale = GlowScale;
        Rectangle frame = texture.Frame(1, Frames, 0, Projectile.frame);
        Rectangle glowFrame = glowTexture.Frame(1, Frames, 0, Projectile.frame);

        if (Projectile.owner == Main.myPlayer && _moveToPositions.Count > 0 && _pathColorMultiplier > 0.02)
        {
            var dashedLine = Assets.Textures.Trails.DashedLine.Asset.Value;

            float totalLength = 0;

            for (int i = 0; i < _moveToPositions.Count - 1; i++)
                totalLength += (_moveToPositions[i + 1] - _moveToPositions[i]).Length();

            var repeats = totalLength / dashedLine.Width;

            Graphics.Graphics.BeginPipeline(1f)
                .DrawTrail(
                    _moveToPositions.ToArray(),
                    _ => 6,
                    a => Color.Lerp(Color.Transparent, Color.DarkGray * 0.5f, MathF.Sin(a * MathF.PI)) * _pathColorMultiplier,
                    Assets.Shaders.Trail.RepeatingTexture.Asset.Value,
                    ("transformMatrix", Graphics.Graphics.WorldTransformMatrix),
                    ("sampleTexture", dashedLine),
                    ("displacementForwards", Main.GlobalTimeWrappedHourly * -0.67f),
                    ("repeats", repeats * 0.5f),
                    ("spriteRotation", 0))
                .Flush();
        }

        using (Main.spriteBatch.Scope())
        {
            var displace = Projectile.Center - Projectile.position;
            var glowDrawOrigin = glowFrame.Size() * 0.5f;
            var drawOrigin = frame.Size() * 0.5f;
            var drawColor = Color.White * colorMult;
            var bloomScale = MathHelper.Lerp(3f, 1, colorMult);

            if (!FlashBangScreenShaderData.DrawingFlashbang)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

                //Main.spriteBatch.Draw(bloomTexture, drawPosition, null, bloomColor, Projectile.rotation * 2f, bloomTexture.Size() * 0.5f, 0.67f, 0, 0);
                Main.spriteBatch.Draw(bloomTexture2, drawPosition, null, bloomColor, MathHelper.WrapAngle(Projectile.rotation * -2.6f), bloomTexture2.Size() * 0.5f, bloomScale * glowFrame.Size() * 0.0015f, 0, 0);
                Main.spriteBatch.Draw(bloomTexture2, drawPosition, null, bloomColor, MathHelper.WrapAngle(Projectile.rotation * 1.9f), bloomTexture2.Size() * 0.5f, bloomScale * glowFrame.Size() * 0.0015f, 0, 0);
                Main.spriteBatch.Draw(bloomTexture2, drawPosition, null, bloomColor * 0.5f, MathHelper.WrapAngle(Projectile.rotation * -0.74f), bloomTexture2.Size() * 0.5f, bloomScale * glowFrame.Size() * 0.0025f, 0, 0);
                Main.spriteBatch.Draw(bloomTexture2, drawPosition, null, bloomColor * 0.5f, MathHelper.WrapAngle(Projectile.rotation * 1.33f), bloomTexture2.Size() * 0.5f, bloomScale * glowFrame.Size() * 0.0025f, 0, 0);

                for (int k = Projectile.oldPos.Length - 1; k >= 0; k--)
                {
                    if (State != CellGellThrowableState.InMotion)
                        break;

                    Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + displace + new Vector2(0f, Projectile.gfxOffY);

                    float ratio = ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                    Color color = drawColor * ratio;
                    Main.spriteBatch.Draw(texture, drawPos, frame, color * ratio, Projectile.oldRot[k], frame.Size() * 0.5f, MathHelper.Lerp(1.2f, 1, colorMult) * Projectile.scale * ratio, 0, 0);
                }

                Main.spriteBatch.Draw(glowTexture, drawPosition, glowFrame, glowColor, Projectile.rotation, glowDrawOrigin, MathHelper.Lerp(1.6f, 1, colorMult) * Projectile.scale * glowScale, 0, 0);
                Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White, Projectile.rotation, drawOrigin, MathHelper.Lerp(1.2f, 1, colorMult) * Projectile.scale, 0, 0);
                Main.spriteBatch.End();
            }

            Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, drawPosition, frame, drawColor, Projectile.rotation, drawOrigin, Projectile.scale * MathHelper.Lerp(0.5f, 1, colorMult), 0, 0);
            Main.spriteBatch.End();
        }

        return false;
    }
}