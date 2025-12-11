using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items;

internal class Jumpjaw : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 25;
        Item.knockBack = 6f;
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.useAnimation = 12;
        Item.useTime = 25;
        Item.width = 36;
        Item.height = 44;
        Item.UseSound = SoundID.Item1;
        Item.DamageType = DamageClass.Melee;
        Item.autoReuse = false;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 1, 50, 0);

        Item.shoot = ModContent.ProjectileType<JumpjawProjectile>();
        Item.shootSpeed = 2.1f;
    }
}

internal class JumpjawProjectile : ModProjectile
{
    private const float BOUNCE = -8.5f;
    private const float SWING_RANGE = 1.66f * (float)Math.PI;
    private const int TOTAL_DURATION = 16;

    private ref float Timer => ref Projectile.ai[0];
    private ref float InitialAngle => ref Projectile.ai[1];

    private Player Owner => Main.player[Projectile.owner];

    public override string Texture => "SpaceEventMod/Assets/Textures/Items/Jumpjaw";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 36;
        Projectile.height = 44;
        Projectile.friendly = true;
        Projectile.timeLeft = 3600;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ownerHitCheck = true;
        Projectile.DamageType = DamageClass.Melee;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((sbyte)Projectile.spriteDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Projectile.spriteDirection = reader.ReadSByte();
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
        float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

        InitialAngle = targetAngle - 0.5f * SWING_RANGE * Projectile.spriteDirection;
    }

    public override void AI()
    {
        bool ownerInvalid = !Owner.active || Owner.dead || Owner.noItems || Owner.CCed;
        bool overDuration = Timer >= TOTAL_DURATION;

        if (ownerInvalid || overDuration)
        {
            Projectile.Kill();
            return;
        }

        Owner.itemAnimation = 2;
        Owner.itemTime = 2;
        Owner.heldProj = Projectile.owner;

        float step = EasingFunctions.OutCirc(Timer / TOTAL_DURATION);
        float progress = MathHelper.SmoothStep(SWING_RANGE, 0, step);

        Projectile.rotation = InitialAngle + Projectile.spriteDirection * progress;

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
        Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2);

        if (Owner.gravDir == -1f)
        {
            Projectile.rotation = 0f - Projectile.rotation;
            armPosition.Y = Owner.Bottom.Y + (Owner.position.Y - armPosition.Y);
        }

        //float scaleStep = EasingFunctions.OutBack(lifetime);
        float scale = MathF.Sin(step * MathF.PI);
        scale = MathHelper.Clamp(scale * 2.5f, 0, 1);

        armPosition.Y += Owner.gfxOffY;
        Projectile.Center = armPosition;
        Projectile.scale = scale * Owner.GetAdjustedItemScale(Owner.HeldItem);

        Timer++;
    }


    public override bool PreDraw(ref Color lightColor)
    {
        bool facingRight = Projectile.spriteDirection > 0;

        Vector2 origin = new Vector2(0, Projectile.height);
        origin.X = facingRight ? 0 : Projectile.width;

        float rotationOffset = facingRight ? MathHelper.ToRadians(45f) : MathHelper.ToRadians(135f);

        SpriteEffects effects = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            default,
            lightColor * Projectile.Opacity,
            Projectile.rotation + rotationOffset,
            origin,
            Projectile.scale,
            effects,
            0);

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 start = Owner.MountedCenter;
        Vector2 end = start + Projectile.rotation.ToRotationVector2() * ((Projectile.Size.Length()) * Projectile.scale);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
    }

    public override void CutTiles()
    {
        Vector2 start = Owner.MountedCenter;
        Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
        Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => OnHitEntity(target);

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => OnHitEntity(target);

    private void OnHitEntity(Entity target)
    {
        Vector2 direction = target.Center - Owner.MountedCenter;

        bool belowThreshold = MathF.Abs(direction.Y) > Owner.height * 0.5f - 6;

        direction = direction.SafeNormalize(Vector2.Zero);

        if (Vector2.Dot(Vector2.UnitY, direction) > 0.5 && belowThreshold)
        {
            Owner.velocity.Y = BOUNCE;
        }
    }
}