using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Splines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;
using static Daybreak.Common.Features.Hooks.GlobalItemHooks;
using static Terraria.Player;

namespace SpaceEventMod.Content.Space.Items
{

    public class ThunderWizardsSmokingPipe : ModItem
    {

        public override void SetDefaults()
        {
            Item.SetWeaponValues(8, 0f);
            Item.DamageType = DamageClass.Magic;
            Item.width = 38;
            Item.height = 76;

            Item.channel = true;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item7;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTurn = true;
            Item.mana = 35;
            Item.shoot = ModContent.ProjectileType<ThunderWizardsSmokingPipeHeld>();
            Item.shootSpeed = 22f;

            Item.useTime = 4;
            Item.useAnimation = 30;
            Item.reuseDelay = 36;

            Item.SetShopValues(ItemRarityID.White, Item.buyPrice(silver: 20));
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }

        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int useTime = (int)(Item.useTime / player.GetTotalAttackSpeed(DamageClass.Magic));
            Projectile.NewProjectileDirect(source, position, Vector2.Zero, Item.shoot, 0, knockback, player.whoAmI, 0, useTime, damage);
            return false;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;

    }

    public class ThunderWizardsSmokingPipeHeld : ModProjectile
    {
        private ref float Charge => ref Projectile.ai[0];
        private float ChargeTime => Projectile.ai[1]*10;
        private bool _primed = false;
        private Vector2 _direction = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 22;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.damage = 2;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
        }

        public override bool PreAI()
        {
            if (!Projectile.TryGetOwner(out Player player))
            {
                Projectile.Kill();
                return false;
            }

            // part of this is from spirit reforged btw; all of this is from siphoning bow btw
            _direction = Vector2.Lerp(_direction, (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero), 0.2f);
            player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation = 2;
            Projectile.Center = player.RotatedRelativePoint(player.Center + _direction * 12);
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = _direction.ToRotation();
            Projectile.netUpdate = true;

          if (player.channel)
            {
                Charge = MathF.Min(Charge + (1 / ChargeTime), 1f);
                Main.NewText(Charge);

                if (!_primed && Charge >= 2)
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

                Vector2 shootVector = Vector2.Lerp(player.velocity/2, _direction.SafeNormalize(Vector2.Zero) * speed, 0.8f);
               
                Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    player.Center,
                    shootVector,
                    ModContent.ProjectileType<ThunderSmoke>(),
                    (int)Projectile.ai[2],
                    knockBack,
                    Projectile.owner,
                    0f,
                    Main.rand.Next(0, 3));
                SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);

                
                Projectile.netUpdate = true;
                Projectile.Kill();
          }

            return false;

        }

    }

    public class ThunderSmoke : ModProjectile
    {
        public override string Texture => "SpaceEventMod/Assets/Textures/EmptyPixel";

        // looked at how fables did the sporethrower projectiile
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Flames);
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2000;
            Projectile.ignoreWater = true;
            Projectile.damage = 10;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public ref float Time => ref Projectile.ai[0];
        
        public override void AI()
        {
            Projectile.velocity *= 0.95f;

            Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.Smoke, Projectile.velocity * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(4f, 4f), Scale: Main.rand.NextFloat(1f, 2f));
            d.noGravity = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            //Slow down on tile collision
            Projectile.velocity = oldVelocity * 0.95f;
            Projectile.position -= Projectile.velocity;
            return false;
        }


    }
}