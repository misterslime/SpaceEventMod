using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
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
        Item.mana = 20;

        Item.shootSpeed = 22f;
        Item.shoot = ModContent.ProjectileType<SiphoningBowHeld>();

        Item.SetWeaponValues(16, 4f);
        Item.DamageType = DamageClass.Magic;
        Item.noMelee = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int useTime = (int)(Item.useTime / player.GetTotalAttackSpeed(DamageClass.Magic));
        Projectile.NewProjectileDirect(source, position, Vector2.Zero, Item.shoot, damage, knockback, player.whoAmI, 0, useTime);
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
                damage,
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
        Thrown,
        ImpaledEnemy,
        ImpaledGround
    }

    public ref float Timer => ref Projectile.ai[0];
    public int ArrowType => (int)Projectile.ai[1];

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;

        Projectile.arrow = true;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.timeLeft = 1200;
    }

    public override void AI()
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


        Projectile.width = Projectile.height = 6;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(3, 1, ArrowType, 0);
        frame.Height = ArrowType switch
        {
            0 => 36,
            1 => 44,
            _ => frame.Height
        };
        Vector2 origin = frame.Center() - frame.Location.ToVector2();

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, 0, 0);

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        Point tilePos = Projectile.Hitbox.Bottom().ToTileCoordinates();

        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        WorldGen.KillTile(tilePos.X, tilePos.Y, fail: true, effectOnly: true);
    }
}
