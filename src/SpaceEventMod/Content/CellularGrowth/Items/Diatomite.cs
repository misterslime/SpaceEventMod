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

internal class Diatomite : CellGelThrowableItem
{
    protected override Vector3 LightColor => Color.GreenYellow.ToVector3() * 0.15f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override int ThrownProjectile => ModContent.ProjectileType<DiatomiteProjectile>();
}

internal class DiatomiteProjectile : CellGelThrowable
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/Diatomite";
    protected override Asset<Texture2D> Glow => Assets.Textures.CellularGrowth.Items.Diatomite_Glow.Asset;
    protected override Color BloomColor => Color.Transparent * 0.35f;
    protected override Color GlowColor => Color.YellowGreen;
    protected override Vector3 LightColor => Color.GreenYellow.ToVector3() * 0.15f;
    protected override int Frames => 4;
    protected override int Damage => 65;
    protected override float Knockback => 8;
    protected override float GlowScale => 1;

    protected override void OnHitTile()
    {
        Projectile.timeLeft = 0;
        Projectile.PrepareBombToBlow();
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<FlashbangDust>(), 0f, 0f, 100, default, 1f);
        dust.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.2f;
        dust.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
        dust.fadeIn = 0;

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
    }
}
