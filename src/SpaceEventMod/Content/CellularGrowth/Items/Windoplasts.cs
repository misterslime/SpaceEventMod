using Microsoft.CodeAnalysis;
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
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static log4net.Appender.ColoredConsoleAppender;
using static Terraria.GameContent.Animations.Actions.NPCs;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class Windoplasts : ModItem
{
    private int _variant;

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(30, 5));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 28;
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

        _variant = Main.rand.Next(0, 5);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int projectile = Projectile.NewProjectile(source, position, velocity, type, damage * 2, knockback, player.whoAmI);
        Main.projectile[projectile].frame = _variant;
        return false;
    }

    public override ModItem Clone(Item item)
    {
        Windoplasts clone = (Windoplasts)base.Clone(item);
        return clone;
    }

    public override void OnCreated(ItemCreationContext context)
    {
        _variant = Main.rand.Next(0, 5);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var texture = TextureAssets.Item[Type].Value;

        var newFrame = new Rectangle(0, 30 * _variant, 24, 28);
        var newOrigin = newFrame.Size() / 2;

        spriteBatch.Draw(texture, position, newFrame, drawColor, 0f, newOrigin, scale, default, 0);

        return false;
    }

    public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type].Value;
        
        var itemFrame = new Rectangle(0, 30 * _variant, 24, 28);
        var drawOrigin = itemFrame.Size() / 2;
        var drawPosition = item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

        spriteBatch.Draw(texture, drawPosition, itemFrame, alphaColor, rotation, drawOrigin, scale, default, 0);
        return false;
    }

    public override void SaveData(TagCompound tag)
    {
        tag[nameof(_variant)] = _variant;
    }

    public override void LoadData(TagCompound tag)
    {
        _variant = tag.GetInt(nameof(_variant));
    }
}

internal class WindoplastProjectile : ModProjectile
{
    public override string Texture => "SpaceEventMod/Assets/Textures/CellularGrowth/Items/Windoplasts";

    private const int DEFAULT_WIDTH_HEIGHT = 15;
    private const int EXPLOSION_WIDTH_HEIGHT = 250;
    private const float KNOCKBACK_STRENGTH = 15f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Type] = true;
        Main.projFrames[Type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.width = DEFAULT_WIDTH_HEIGHT;
        Projectile.height = DEFAULT_WIDTH_HEIGHT;
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

        Projectile.Resize(DEFAULT_WIDTH_HEIGHT, DEFAULT_WIDTH_HEIGHT);

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