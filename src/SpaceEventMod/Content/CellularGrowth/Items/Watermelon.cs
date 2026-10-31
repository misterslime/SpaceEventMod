using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace SpaceEventMod.Content.CellularGrowth.Items;

    
 public class Watermelon : ModItem
{
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;


            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(-1, 3)
            {
                NotActuallyAnimating = true
            });


            ItemID.Sets.DrinkParticleColors[Type] = [
                new Color(245, 66, 111),
                new Color(232, 21, 74),
                new Color(184, 7, 52)
            ];

            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
 
            Item.DefaultToFood(36, 34, BuffID.WellFed3, 57600); 
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.value = Item.buyPrice(0, 3);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item2 with { Pitch = -1.8f };

           // SoundEngine.PlaySound(SoundID.Item2 with { Pitch = -10f });
        }
}

public class WatermelonBoulder : ModProjectile
{

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.Boulder);
        Projectile.height = 36;
        Projectile.width = 34;
        Projectile.friendly = false;
        Projectile.tileCollide = true;
        Projectile.aiStyle = ProjAIStyleID.Boulder;

    }

    public override void AI()
    {
        Projectile.rotation += 0.1f * Projectile.velocity.Y;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion);

        return true;
    }

    public override void OnKill(int timeLeft)
    {
        var spawnPos = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);

        spawnPos = Projectile.Center;

        Item.NewItem(Projectile.GetSource_Death(), spawnPos, Projectile.Size, ModContent.ItemType<WatermelonSlice>(), 3);

        float velocityRotation = Main.rand.NextFloat(MathHelper.TwoPi);

        Dust dust = Dust.NewDustDirect(spawnPos, 0, 0, DustID.RedStarfish, 0f, 0f, 100, default, 0f);

        int[] gores = new int[8];

        for (int i = 0; i < 8; i++)
        {
            gores[i] = Mod.Find<ModGore>($"MelonGore{Projectile.frame}_{i}").Type;
        }
        // did you know cut of pieces of fruit make great reference for blood and gore? That way if you're uncomfortable looking at things like that, you can just reference those and manage to create a similar body horror feeling. Alongside that, I'm learning that people also use sounds of fruit being broken up to emulate the sounds of bones breaking, and blood squelching, it really makes you think about the relationship between fruit and the human body, pretty neat stuff!

        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(1.0f, -1.0f).RotatedBy(velocityRotation), gores[0], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(1.0f, -1.0f).RotatedBy(velocityRotation), gores[1], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(1.0f, -1.0f).RotatedBy(velocityRotation), gores[2], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-1.5f, -1.5f).RotatedBy(velocityRotation), gores[3], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-1.5f, -1.5f).RotatedBy(velocityRotation), gores[4], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-1.5f, -1.5f).RotatedBy(velocityRotation), gores[5], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-1.5f, -1.5f).RotatedBy(velocityRotation), gores[6], 1f);
        Gore.NewGore(Projectile.GetSource_FromThis(), spawnPos, new Vector2(-1.5f, -1.5f).RotatedBy(velocityRotation), gores[7], 1f);
    }
}

public class WatermelonSlice : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;


        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(-1, 3)
        {
            NotActuallyAnimating = true
        });


        ItemID.Sets.DrinkParticleColors[Type] = [
            new Color(245, 66, 111),
                new Color(232, 21, 74),
                new Color(184, 7, 52)
        ];

        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults()
    {

        Item.DefaultToFood(32, 22, BuffID.WellFed, 5760);
        Item.value = Item.buyPrice(0, 3);
        Item.rare = ItemRarityID.Blue;
    }
}
