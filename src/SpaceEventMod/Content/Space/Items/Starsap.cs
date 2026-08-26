using Microsoft.Xna.Framework;
using SpaceEventMod.Common.StarsapCoating;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Items;

internal class Starsap : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<StarsapProjectile>();
        Item.width = 8;
        Item.height = 28;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.value = Item.buyPrice(0, 0, 20, 0);
        Item.rare = ItemRarityID.Blue;
    }
}

internal class StarsapProjectile : ModProjectile
{
    public override string Texture => "SpaceEventMod/Assets/Textures/Space/Items/Starsap";

    private const int DefaultWidthHeight = 15;
    private const int ExplosionWidthHeight = 250;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = DefaultWidthHeight;
        Projectile.height = DefaultWidthHeight;
        Projectile.friendly = true;
        Projectile.penetrate = -1;

        Projectile.timeLeft = 300;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.timeLeft = 0;
        Projectile.PrepareBombToBlow();
        return true;
    }

    public override void PrepareBombToBlow()
    {
        Projectile.tileCollide = false;
        Projectile.alpha = 255;

        Projectile.Resize(ExplosionWidthHeight, ExplosionWidthHeight);
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        Projectile.Resize(DefaultWidthHeight, DefaultWidthHeight);

        if (Projectile.owner == Main.myPlayer)
        {
            var explosionRadius = 7;
            var minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
            var maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
            var minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
            var maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

            Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);


            for (var i = minTileX; i <= maxTileX; i++)
            {
                for (var j = minTileY; j <= maxTileY; j++)
                {
                    var tile = Framing.GetTileSafely(i, j);

                    if (!(tile.active() && Main.tileSolid[tile.type]))
                        continue;

                    StarsapCoatingSystem.CoatTile(i, j, true);
                }
            }
        }
    }
}
