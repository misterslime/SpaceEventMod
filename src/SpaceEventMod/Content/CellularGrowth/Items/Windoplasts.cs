using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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

        Item.shootSpeed = 18f;
        Item.shoot = ModContent.ProjectileType<WindoplastProjectile>();
        Item.consumable = true;
        Item.UseSound = SoundID.Item1;
        Item.useAnimation = 8;
        Item.useTime = 8;
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

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type].Value;
        
        var itemFrame = new Rectangle(0, 30 * _variant, 24, 28);
        var drawOrigin = itemFrame.Size() / 2;
        var drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

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

    private const int BURST_RADIUS = 250;
    private const float KNOCKBACK_STRENGTH = 15f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Type] = true;
        Main.projFrames[Type] = 5;
    }

    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;
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

        Projectile.rotation += Projectile.velocity.X * 0.01f;

        if (Projectile.owner != Main.myPlayer)
            return;

        // collide with npcs
        foreach (var npc in Main.ActiveNPCs)
        {
            if (!npc.Hitbox.Intersects(Projectile.Hitbox))
                continue;

            Projectile.PrepareBombToBlow();
            Projectile.ai[0] = 1;
        }
    }


    public override void PrepareBombToBlow()
    {
        Projectile.timeLeft = 0;
        Projectile.alpha = 255;
        Projectile.netUpdate = true;
        Projectile.tileCollide = false;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item45, Projectile.position);
        SoundEngine.PlaySound(SoundID.NPCDeath11, Projectile.position);

        if (Projectile.owner != Main.myPlayer)
            return;

        var spawnPos = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);

        spawnPos = Projectile.Center;

        Dust dust = Dust.NewDustDirect(spawnPos, 0, 0, ModContent.DustType<Azasplosion>(), 0f, 0f, 100, default, 2f);
        dust.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.2f;
        //dust.velocity = Vector2.Zero;
        //dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        dust.rotation = 0f;
        dust.fadeIn = 4 * Main.rand.Next(0, 3);
        dust.customData = (int)Main.rand.Next(0, 3);

        for (int i = 0; i < 5; i++)
        {
            float radius = Main.rand.NextFloat(1, 2);

            var speed = Main.rand.NextVector2Unit();

            dust = Dust.NewDustDirect(spawnPos, 0, 0, ModContent.DustType<Azasplosion>(), 0f, 0f, 100, default, 2f);
            dust.velocity = speed * radius + Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.2f;
            //dust.velocity = Vector2.Zero;
            //dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.rotation = 0f;
            dust.fadeIn = 4 * Main.rand.Next(0, 3);
            dust.customData = (int)Main.rand.Next(0, 3);
        }

        float velocityRotation = Main.rand.NextFloat(MathHelper.TwoPi);

        int[] gores = new int[4];

        for (int i = 0; i < 4; i++)
        {
            gores[i] = Mod.Find<ModGore>($"WindoplastGore{Projectile.frame}_{i}").Type;
        }

        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(1.5f, 1.5f).RotatedBy(velocityRotation), gores[0], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-1.5f, 1.5f).RotatedBy(velocityRotation), gores[1], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(1.5f, -1.5f).RotatedBy(velocityRotation), gores[2], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-1.5f, -1.5f).RotatedBy(velocityRotation), gores[3], 1f);

        foreach (var npc in Main.ActiveNPCs)
        {
            Vector2 kbVector = npc.Center - Projectile.Hitbox.Bottom();
            float distance = InvLerp(BURST_RADIUS * 0.5f, 0, kbVector.Length());

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
            float distance = InvLerp(BURST_RADIUS * 0.5f, 0, kbVector.Length());

            kbVector = kbVector.SafeNormalize(Vector2.Zero);
            kbVector -= Vector2.UnitY;
            kbVector = kbVector.SafeNormalize(Vector2.Zero);

            player.velocity += kbVector * KNOCKBACK_STRENGTH * MathHelper.Clamp(distance, 0, 1);
        }
    }

    private float InvLerp(float a, float b, float v) => (v - a) / (b - a);

}