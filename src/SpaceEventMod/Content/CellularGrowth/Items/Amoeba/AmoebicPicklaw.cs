using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Splines;
using SpaceEventMod.Core.Animation.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.Actions.NPCs;

namespace SpaceEventMod.Content.CellularGrowth.Items.Amoeba;

internal class AmoebicPicklaw : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 52;
        Item.height = 50;
        Item.SetShopValues((Terraria.Enums.ItemRarityColor)ItemRarityID.Blue, Item.buyPrice(silver: 20));

        Item.channel = true;
        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<AmoebicPicklawProjectile>();
        Item.UseSound = SoundID.Item7;
        Item.useTime = Item.useAnimation = 40;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.Shoot;
    }
}

internal class AmoebicPicklawProjectile : ModProjectile
{
    private const int DEFAULT_WIDTH_HEIGHT = 26;
    private const int EXPLOSION_WIDTH_HEIGHT = 250;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = DEFAULT_WIDTH_HEIGHT;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.aiStyle = -1;
    }

    public override bool PreAI()
    {
        Projectile.ai[1]++;

        float maxDragSpeed = 18;
        float dragAcceleration = 2f;

        if (!Projectile.TryGetOwner(out Player player))
        {
            Projectile.Kill();
            return false;
        }

        player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
        player.heldProj = Projectile.whoAmI;
        player.itemTime = player.itemAnimation = 2;

        if (Main.myPlayer != player.whoAmI)
            return false;

        if (player.channel)
        {
            if (Projectile.ai[0] == 0f)
            {
                float lerpAmount = MathHelper.Clamp(Projectile.ai[1] / 20f, 0, 1);

                Projectile.Center = Vector2.Lerp(player.Center, Main.MouseWorld, EasingFunctions.OutQuart( lerpAmount));
                Projectile.netUpdate = true;

                if (Projectile.ai[1] >= 20f)
                    Projectile.ai[0] = 1f;
            }

            if (Projectile.ai[0] == 1f)
            {
                var mouseProjectile = Main.MouseWorld - Projectile.Center;

                var toMouse = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero);
                var toClaw = (Projectile.Center - player.Center).SafeNormalize(Vector2.Zero);

                var angleBetween = Vector2.Dot(toMouse, toClaw);
                angleBetween = MathF.Acos(angleBetween);

                //Projectile.velocity *= 0.92f;
                //Projectile.velocity += mouseProjectile.SafeNormalize(Vector2.Zero) * angleBetween;

                Projectile.Center = Vector2.Lerp(Projectile.Center, Main.MouseWorld, 0.12f);

                Projectile.ai[1] = 0;
                Projectile.netUpdate = true;
            }

            //Projectile.Center = Vector2.Lerp(Projectile.Center, Main.MouseWorld, 0.2f);
            //Projectile.Center = Main.MouseWorld;
            //Projectile.netUpdate = true;
        }
        else
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.2f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center, 0.2f);
            
            if (player.Hitbox.Intersects(Projectile.Hitbox))
                Projectile.Kill();
        }

        return true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        // Play explosion sound
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        // Smoke Dust spawn
        for (int i = 0; i < 50; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
            dust.velocity *= 1.4f;
        }

        // Fire Dust spawn
        for (int i = 0; i < 80; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
            dust.noGravity = true;
            dust.velocity *= 5f;
            dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
            dust.velocity *= 3f;
        }

        // Large Smoke Gore spawn
        for (int g = 0; g < 2; g++)
        {
            var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
            Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y += 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X += 1.5f;
            gore.velocity.Y -= 1.5f;
            gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
            gore.scale = 1.5f;
            gore.velocity.X -= 1.5f;
            gore.velocity.Y -= 1.5f;
        }

        // Finally, actually explode the tiles and walls. Run this code only for the owner
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

        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!Projectile.TryGetOwner(out Player player))
            return false;

        var midControlPoint = Vector2.Lerp(player.Center, Projectile.Center, 0.7f);

        if (player.channel)
            midControlPoint += (Main.MouseWorld - Projectile.Center) * 0.45f;

        midControlPoint -= Main.screenPosition;

        var trailPoints = new List<Vector2>();

        ReadOnlySpan<Vector2> controlPoints = [player.Center - Main.screenPosition, midControlPoint, Projectile.Center - Main.screenPosition];
        using (var curve = new BezierCurve(controlPoints))
            trailPoints = curve.GetPoints(20);

        for (int i = 0; i < trailPoints.Count - 1; i++)
        {
            Main.spriteBatch.DrawLine(trailPoints[i], trailPoints[i + 1], Color.White, 4);
        }

        Projectile.rotation = (trailPoints[trailPoints.Count - 1] - trailPoints[trailPoints.Count - 2]).SafeNormalize(Vector2.Zero).ToRotation();
        Projectile.rotation += MathHelper.PiOver2;

        return true;
    }
}