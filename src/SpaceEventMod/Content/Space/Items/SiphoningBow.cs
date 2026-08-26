using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Space.LevelElements;
using SpaceEventMod.Core.Animation.Splines;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Player;

namespace SpaceEventMod.Content.Space.Items;

internal class SiphoningBow : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 76;
        Item.SetShopValues(ItemRarityID.White, Item.buyPrice(silver: 20));
        Item.noUseGraphic = true;

        Item.channel = true;
        Item.autoReuse = true;
        Item.useTime = Item.useAnimation = 40;
        Item.UseSound = SoundID.Item7;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTurn = true;
        Item.mana = 35;

        Item.shootSpeed = 22f;
        Item.shoot = ModContent.ProjectileType<SiphoningBowHeld>();

        Item.SetWeaponValues(16, 4f);
        Item.DamageType = DamageClass.Magic;
        Item.noMelee = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int useTime = (int)(Item.useTime / player.GetTotalAttackSpeed(DamageClass.Magic));
        Projectile.NewProjectileDirect(source, position, Vector2.Zero, Item.shoot, 0, knockback, player.whoAmI, 0, useTime, damage);
        return false;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.GoldBar, 5)
            .AddIngredient(ModContent.ItemType<Starsap>(), 12)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.PlatinumBar, 5)
            .AddIngredient(ModContent.ItemType<Starsap>(), 12)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

internal class SiphoningBowHeld : ModProjectile
{
    private ref float Charge => ref Projectile.ai[0];
    private float ChargeTime => Projectile.ai[1];

    private bool _primed = false;
    private Vector2 _direction = Vector2.Zero;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 19;
    }

    public override void SetDefaults()
    {
        Projectile.width = 56;
        Projectile.height = 92;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_primed);
        writer.WriteVector2(_direction);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _primed = reader.ReadBoolean();
        _direction = reader.ReadVector2();
    }

    public override bool PreAI()
    {
        if (!Projectile.TryGetOwner(out Player player))
        {
            Projectile.Kill();
            return false;
        }

        // part of this is from spirit reforged btw
        _direction = Vector2.Lerp(_direction, (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero), 0.2f);
        player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
        player.heldProj = Projectile.whoAmI;
        player.itemTime = player.itemAnimation = 2;
        Projectile.Center = player.RotatedRelativePoint(player.Center + _direction * 12);
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation = _direction.ToRotation();
        Projectile.netUpdate = true;

        CompositeArmStretchAmount frontStretch = Charge switch
        {
            < 0.25f => CompositeArmStretchAmount.Full,
            < 0.5f => CompositeArmStretchAmount.ThreeQuarters,
            < 0.75f => CompositeArmStretchAmount.Quarter,
            _ => CompositeArmStretchAmount.None
        };

        player.SetCompositeArmFront(true, frontStretch, player.itemRotation);
        player.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, player.itemRotation);

        if (player.channel)
        {
            Charge = MathF.Min(Charge + (1 / ChargeTime), 1f);
            Projectile.frame = (int)MathHelper.Lerp(1, 12, Charge);

            if (!_primed && Charge >= 1)
            {
                SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
                _primed = true;
            }
        }
        else
        {
            Item playerWeapon = player.HeldItem;

            float speed = playerWeapon.shootSpeed * Charge;
            int damage = (int)(Projectile.damage * Charge);
            float knockBack = Projectile.knockBack * Charge;
            Vector2 shootVector = Vector2.Lerp(player.velocity, _direction.SafeNormalize(Vector2.Zero) * speed, Charge);

            Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(), 
                player.Center, shootVector, 
                ModContent.ProjectileType<SiphoningBowArrow>(),
                (int)Projectile.ai[2],
                knockBack, 
                Projectile.owner,
                0f,
                Main.rand.Next(0, 3));
            SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);

            Projectile.frame = 0;
            Projectile.netUpdate = true;
            Projectile.Kill();
        }

        return false;
    }
}

internal class SiphoningBowArrow : ModProjectile
{
    private enum ArrowState
    {
        Flying,
        ManaSteal,
        Impaled
    }

    public ref float Timer => ref Projectile.ai[0];
    public int ArrowType => (int)Projectile.ai[1];

    private ArrowState State
    {
        get => (ArrowState)Projectile.ai[2];
        set => Projectile.ai[2] = (float)value;
    }

    private int _targetWhoAmI = -1;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 6;

        Projectile.arrow = true;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.timeLeft = 1200;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_targetWhoAmI);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _targetWhoAmI = reader.Read();
    }

    public override void AI()
    {
        switch (State)
        {
            case ArrowState.Flying:
                Flying();
                break;
            case ArrowState.ManaSteal:
                ManaSteal();
                break;
            case ArrowState.Impaled:
                Impaled();
                break;
            default:
                break;
        }

        if (State != ArrowState.ManaSteal)
        {
            Projectile.drawLayer = ProjectileDrawLayerID.BehindNPCsAndTiles;
            return;
        }
        else
        {
            // If attached to an NPC, draw behind tiles (and the npc) if that NPC is behind tiles, otherwise just behind the NPC.
            if (_targetWhoAmI >= 0 && _targetWhoAmI < 200 && Main.npc[_targetWhoAmI].active)
                Projectile.drawLayer = Main.npc[_targetWhoAmI].behindTiles ? ProjectileDrawLayerID.BehindNPCsAndTiles : ProjectileDrawLayerID.BehindNPCs;
        }
    }

    private void Flying()
    {
        Timer += 1f;
        if (Timer >= 15f)
        {
            Timer = 15f;
            Projectile.velocity.Y += 0.35f;
            Projectile.velocity.X *= 0.99f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }

        if (Main.myPlayer != Projectile.owner)
            return;

        Vector2 heading = Projectile.velocity;
        heading.Normalize();

        Vector2 worldPos = Projectile.Center;
        Point tilePos = worldPos.ToTileCoordinates();

        Tile tile = Main.tile[tilePos];

        if (!tile.HasUnactuatedTile || !Main.tileSolid[tile.TileType] || Main.player[Projectile.owner].IsBlacklistedForGrappling(tilePos) || TileID.Sets.Platforms[tile.TileType])
            return;

        Projectile.velocity = Vector2.Zero;
        Timer = 32;
        State = ArrowState.Impaled;
        Projectile.damage = 0;
        Projectile.Center = worldPos + Vector2.One * 8f;

        WorldGen.KillTile(tilePos.X, tilePos.Y, fail: true, effectOnly: true);
        SoundEngine.PlaySound(SoundID.Dig, worldPos);

        Rectangle? tileVisualHitbox = WorldGen.GetTileVisualHitbox(tilePos.X, tilePos.Y);
        if (tileVisualHitbox.HasValue)
            Projectile.Center = tileVisualHitbox.Value.Center.ToVector2();

        Projectile.netUpdate = true;
    }

    private void ManaSteal()
    {
        Timer -= 1f;

        if (Timer <= 2)
            Timer = 2;

        if (_targetWhoAmI < 0 || _targetWhoAmI >= 200)
        {
            State = ArrowState.Flying;
            return;
        }
        else if (Main.npc[_targetWhoAmI].active && !Main.npc[_targetWhoAmI].dontTakeDamage)
        {
            Projectile.Center = Main.npc[_targetWhoAmI].Center - (Projectile.velocity * 2f).RotatedBy(Main.npc[_targetWhoAmI].rotation);
            Projectile.gfxOffY = Main.npc[_targetWhoAmI].gfxOffY;
        }
        else
        {
            State = ArrowState.Flying;
            return;
        }

        if (Main.myPlayer != Projectile.owner)
            return;

        Player owner = Main.player[Projectile.owner];

        owner.TryGetModPlayer<SiphoningBowPlayer>(out var result);
        result.ManaStealArrows++;
    }

    private void Impaled()
    {
        Timer -= 1f;

        if (Timer <= 2)
            Timer = 2;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (State != ArrowState.Flying || !target.active)
            return;

        Timer = 32;
        State = ArrowState.ManaSteal;
        _targetWhoAmI = target.whoAmI;

        Projectile.velocity = (target.Center - Projectile.Center) * 0.25f;
        Projectile.velocity.Y *= -1;
        Projectile.damage = 0;
        Projectile.netUpdate = true;

        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }

    public override void OnKill(int timeLeft)
    {
        Point tilePos = Projectile.Hitbox.Bottom().ToTileCoordinates();

        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        WorldGen.KillTile(tilePos.X, tilePos.Y, fail: true, effectOnly: true);
    }

    public override bool PreDraw(Player player, ref Color lightColor)
    {
        // framing
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(3, 1, ArrowType, 0);
        int redHeight = 36;
        int yellowHeight = 44;

        frame.Height = ArrowType switch
        {
            0 => redHeight,
            1 => yellowHeight,
            _ => frame.Height
        };

        float rotationOffset = 0;

        if (State != ArrowState.Flying)
            rotationOffset = (float)Math.Sin(Timer + MathF.PI) / ((30 - Timer) + MathF.PI);

        rotationOffset *= 0.5f;

        if (_targetWhoAmI >= 0 && _targetWhoAmI < 200 && State == ArrowState.ManaSteal)
            rotationOffset += Main.npc[_targetWhoAmI].rotation;

        Vector2 origin = Vector2.Lerp(frame.Center(), frame.Top(), 1f) - frame.Location.ToVector2();

        float rotation = Projectile.rotation + rotationOffset;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, rotation, origin, Projectile.scale, 0, 0);

        if (State != ArrowState.ManaSteal)
            return false;

        Vector2 rotationVector = (rotation + MathHelper.PiOver2).ToRotationVector2();

        Vector2 start = Projectile.Center + rotationVector * frame.Height;
        Vector2 end = player.Center;

        Vector2 middle = start + (Vector2.Lerp(start, end, 0.5f) - start).Length() * (rotation + MathHelper.PiOver2).ToRotationVector2();


        var segments = 25;
        var trailPoints = new List<Vector2>(segments + 1);

        ReadOnlySpan<Vector2> controlPoints = new Vector2[] { start, middle, end };
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(segments + 1);

        Color insideColor = ArrowType switch
        {
            0 => Color.Red,
            1 => Color.Yellow,
            _ => Color.Cyan
        };

        Color outlineColor = ArrowType switch
        {
            0 => Color.Red,
            1 => Color.Orange,
            _ => Color.Blue
        };

        Core.Graphics.Graphics.BeginPipeline(0.5f)
            .DrawBasicTrail(
                trailPoints.ToArray(),
                r => 2 + 4 * MathHelper.Clamp(MathF.Sin(r * MathHelper.Pi * 5 - 16 * Main.GlobalTimeWrappedHourly), 0, 1),
                Assets.Textures.WhitePixel.Asset.Value,
                r => insideColor * 0.3f)
            .ApplyOutline(outlineColor * 0.6f)
            .Schedule(RenderLayer.AfterTiles);

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 heading = Projectile.velocity;
        heading.Normalize();

        if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);

        int redHeight = 36;
        int yellowHeight = 44;
        int blueHeight = 58;

        int projectileHeight = ArrowType switch
        {
            0 => redHeight,
            1 => yellowHeight,
            _ => blueHeight
        };

        float collisionPoint = 0f;

        Vector2 start = Projectile.Center;
        Vector2 end = Projectile.Center - heading * projectileHeight;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 16, ref collisionPoint);
    }
}

public class SiphoningBowPlayer : ModPlayer
{
    public int ManaStealArrows { get; set; } = 0;

    public override void UpdateDead()
    {
        ManaStealArrows = 0;
    }

    public override void PostUpdateMiscEffects()
    {
        if (ManaStealArrows > 0)
        {
            Player.manaRegenBonus += 1 * ManaStealArrows;
            Player.manaRegenBuff = true;
        }

        ManaStealArrows = 0;
    }
}