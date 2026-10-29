using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Common.BaseTypes;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class BacteriaBomb : CellGelThrowableItem
{
    protected override Vector3 LightColor => Color.Purple.ToVector3() * 0.5f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override int ThrownProjectile => ModContent.ProjectileType<BacteriaBombProjectile>();
}

internal class BacteriaBombProjectile : CellGelThrowable
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/BacteriaBomb";
    protected override Asset<Texture2D> Glow => Assets.Textures.CellularGrowth.Items.BacteriaBomb_Glow.Asset;
    protected override Color BloomColor => Color.Purple * 0.75f;
    protected override Color GlowColor => Color.Coral;
    protected override Vector3 LightColor => Color.Purple.ToVector3() * 0.5f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override float GlowScale => 1;

    protected override void OnHitTile()
    {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        if (Projectile.owner == Main.myPlayer)
        {
            int explosionRadius = 7; // Bomb: 4, Dynamite: 7, Explosives & TNT Barrel: 10
            int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
            int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
            int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
            int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

            // Ensure that all tile coordinates are within the world bounds
            Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

            // These 2 methods handle actually mining the tiles and walls while honoring tile explosion conditions
            bool explodeWalls = Projectile.ShouldWallExplode(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY);
            Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, explodeWalls);
        }

        Projectile.timeLeft = 0;
        Projectile.PrepareBombToBlow();

        Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<FlashbangDust>(), 0f, 0f, 100, default, 1f);
        dust.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.2f;
        dust.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
        dust.fadeIn = 0;
    }
}