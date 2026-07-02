using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Content.Space.Mechanics.StarsapCoating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class Windoplasts : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);

        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<WindoplastProjectile>();
        Item.consumable = true;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.Swing;
    }
}

internal class WindoplastProjectile : ModProjectile
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/Windoplasts";

    private const int DefaultWidthHeight = 15;
    private const int EXPLOSION_WIDTH_HEIGHT = 250;
    private const float KNOCKBACK_STRENGTH = 15f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = DefaultWidthHeight;
        Projectile.height = DefaultWidthHeight;
        Projectile.friendly = true;
        Projectile.penetrate = 1;

        Projectile.timeLeft = 300;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.owner == Main.myPlayer)
            Projectile.PrepareBombToBlow();

        return true;
    }

    public override void AI()
    {
        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
            Projectile.PrepareBombToBlow();

        // collide with npcs
        if (Projectile.owner == Main.myPlayer)
        {
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.Hitbox.Intersects(Projectile.Hitbox))
                {
                    Projectile.PrepareBombToBlow();
                    Projectile.ai[0] = 1;
                }
            }
        }

        Projectile.rotation += Projectile.velocity.X * 0.01f;
    }


    public override void PrepareBombToBlow()
    {
        Projectile.timeLeft = 0;
        Projectile.tileCollide = false;
        Projectile.alpha = 255;

        Projectile.Resize(EXPLOSION_WIDTH_HEIGHT, EXPLOSION_WIDTH_HEIGHT);
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

        Projectile.Resize(DefaultWidthHeight, DefaultWidthHeight);

        if (Projectile.owner != Main.myPlayer)
            return;

        // Example Mod code
        // Smoke Dust spawn
        for (int i = 0; i < 50; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
            dust.velocity *= 1.4f;
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

        foreach (var npc in Main.ActiveNPCs)
        {
            Vector2 kbVector = npc.Center - Projectile.Hitbox.Bottom();
            float distance = InvLerp(EXPLOSION_WIDTH_HEIGHT * 0.5f, 0, kbVector.Length());

            if (Projectile.ai[0] == 1)
                kbVector = Projectile.velocity;

            kbVector = kbVector.SafeNormalize(Vector2.Zero);
            kbVector -= Vector2.UnitY;
            kbVector = kbVector.SafeNormalize(Vector2.Zero);

            npc.velocity += kbVector * KNOCKBACK_STRENGTH * npc.knockBackResist * MathHelper.Clamp(distance, 0, 1);
        }

        foreach (var player in Main.ActivePlayers)
        {
            Vector2 kbVector = player.Center - Projectile.Hitbox.Bottom();
            float distance = InvLerp(EXPLOSION_WIDTH_HEIGHT * 0.5f, 0, kbVector.Length());

            kbVector = kbVector.SafeNormalize(Vector2.Zero);
            kbVector -= Vector2.UnitY;
            kbVector = kbVector.SafeNormalize(Vector2.Zero);

            player.velocity += kbVector * KNOCKBACK_STRENGTH * MathHelper.Clamp(distance, 0, 1);
        }


        int peeb = 5;
        peeb -= 2;
    }

    private float InvLerp(float a, float b, float v) => (v - a) / (b - a);

}