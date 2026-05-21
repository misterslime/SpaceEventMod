using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
        Item.rare = ItemRarityID.White;
        Item.value = Item.buyPrice(silver: 20);
        Item.noUseGraphic = true;

        Item.channel = true;
        Item.autoReuse = true;
        Item.useTime = Item.useAnimation = 40;
        Item.UseSound = SoundID.Item7;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTurn = true;

        Item.shootSpeed = 15f;
        Item.shoot = ModContent.ProjectileType<SiphoningBowHeld>();
        Item.useAmmo = AmmoID.Arrow;

        Item.damage = 16;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 4f;
        Item.noMelee = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int useTime = (int)(Item.useTime / player.GetTotalAttackSpeed(DamageClass.Ranged));
        Projectile.NewProjectileDirect(source, position, Vector2.Zero, Item.shoot, damage, knockback, player.whoAmI, 0, useTime, type);
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
    private int AmmoType => (int)Projectile.ai[2];

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
        Projectile.DamageType = DamageClass.Ranged;
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
            Vector2 shootVector = _direction.SafeNormalize(Vector2.Zero) * speed * Charge;

            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), player.Center, shootVector, AmmoType, damage, knockBack, Projectile.owner);
            SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);

            Projectile.frame = 0;
            Projectile.netUpdate = true;
            Projectile.Kill();
        }

        return false;
    }
}

internal class SiphoningBowArrows : GlobalProjectile
{
    private bool _siphonArrow = false;
    private int _type = 0; // 0 = red, 1 = yellow, 2 = blue

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.friendly && entity.DamageType == DamageClass.Ranged;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        _siphonArrow = false;

        if (source is EntitySource_Parent { Entity: Projectile proj } && proj != null && proj.ModProjectile is SiphoningBowHeld)
        {
            _siphonArrow = true;
            _type = Main.rand.Next(0, 3);
        }
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (!_siphonArrow)
            return true;

        Texture2D tex = Assets.Textures.Space.Items.SiphoningBowArrows.Asset.Value;
        Rectangle frame = tex.Frame(3, 1, _type, 0);
        Vector2 origin = frame.Center() - frame.Location.ToVector2();

        Main.EntitySpriteDraw(tex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, origin, projectile.scale, 0, 0);

        return false;
    }
}
