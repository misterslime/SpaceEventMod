using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Splines;
using SpaceEventMod.Content.Miscellaneous.Dusts;
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
            Item.UseSound = SoundID.Item34;
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

        private const float FANNING_DEVIATION = MathHelper.PiOver4 * 0.25f;
        private const int LIFETIME_AFTER_CHANNELED = 45;
        private const int SHOOT_INTERVAL = 3; // number of projectiles shot should be roughly LIFETIME_AFTER_CHANNELED / SHOOT_INTERVAL
        private const int VELOCITY_HALF_RANGE = 15;
        private ref float Charge => ref Projectile.ai[0];
        private float ChargeTime => Projectile.ai[1] * 10;
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


            // part of this is from spirit reforged btw
            player.ChangeDir(_direction.X > 0 ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation = 2;
            Projectile.Center = player.RotatedRelativePoint(player.Center + _direction * 12);
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = _direction.ToRotation();
            Projectile.netUpdate = true;

            if (player.channel)
            {
                _direction = Vector2.Lerp(_direction, (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero), 0.2f);
                Charge = MathF.Min(Charge + (1 / ChargeTime), 1f);
                Projectile.frame = (int)MathHelper.Lerp(1, 12, Charge);
                Projectile.timeLeft = LIFETIME_AFTER_CHANNELED;
                Main.NewText(Charge);

                if (!_primed && Charge >= 1)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
                    _primed = true;
                }
            }
            else if (Projectile.timeLeft % (SHOOT_INTERVAL / Charge) == 0)
            {
                Item playerWeapon = player.HeldItem;

                float speed = playerWeapon.shootSpeed * Charge + Main.rand.NextFloat(-1, 1) * VELOCITY_HALF_RANGE;
                int damage = (int)(Projectile.damage * Charge);
                float knockBack = Projectile.knockBack * Charge;

                float angle = _direction.ToRotation();

                float x1 = 1f - Main.rand.NextFloat();
                float x2 = 1f - Main.rand.NextFloat();

                float y1 = MathF.Sqrt(-2f * MathF.Log(x1)) * MathF.Cos(2f * MathF.PI * x2);

                angle = MathHelper.WrapAngle(angle + FANNING_DEVIATION * y1);


                Vector2 shootVector = Vector2.Lerp(player.velocity, angle.ToRotationVector2() * speed, Charge);



                Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    player.Center, shootVector,
                    ModContent.ProjectileType<ThunderSmoke>(),
                    (int)Projectile.ai[2],
                    knockBack,
                    Projectile.owner,
                    0f,
                    Main.rand.Next(0, 3));
                SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);

                Projectile.frame = 0;
                Projectile.netUpdate = true;
                //Projectile.Kill();
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
            Projectile.width = 60;
            Projectile.height = 0;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2000;
            Projectile.ignoreWater = true;
            Projectile.damage = 10;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.95f;

            var spawnPos = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);

            Dust d = Dust.NewDustDirect(spawnPos, 0, 0, ModContent.DustType<Azasplosion>(), 0f, 0f, 100, default, 2f);
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
