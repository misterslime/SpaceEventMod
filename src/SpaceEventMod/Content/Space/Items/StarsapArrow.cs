using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Items;

internal class StarsapArrow : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
    }

    public override void SetDefaults()
    {
        Item.width = 14;
        Item.height = 36;

        Item.damage = 9;
        Item.DamageType = DamageClass.Ranged;

        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.knockBack = 3f;
        Item.value = Item.sellPrice(copper: 16);
        Item.shoot = ModContent.ProjectileType<StarsapArrowProjectile>();
        Item.shootSpeed = 5f;
        Item.ammo = AmmoID.Arrow;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.WoodenArrow, 40)
            .AddIngredient<Starsap>()
            .Register();
    }
}

internal class StarsapArrowProjectile : ModProjectile
{
    public override string Texture => ModContent.GetInstance<StarsapArrow>().Texture;

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10; // The height of projectile hitbox

        Projectile.arrow = true;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1200;
    }

    public override void AI()
    {
        var displace = 30 * Vector2.UnitX.RotatedBy(Projectile.rotation + MathHelper.PiOver2);

        for (int i = 0; i < 2; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position + displace, Projectile.width, Projectile.height, DustID.Pixie);
            dust.noGravity = true;
            dust.velocity *= 1.5f;
            dust.scale *= 0.9f;
        }

        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        // Apply gravity after a quarter of a second
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] < 15f)
            return;

        Projectile.velocity += Vector2.UnitY * 0.15f;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);
    }
}
