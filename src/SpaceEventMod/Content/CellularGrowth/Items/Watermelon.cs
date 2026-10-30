using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        Item.NewItem(Projectile.GetSource_Death(), Projectile.position, Projectile.Size, ModContent.ItemType<WatermelonSlice>(), 3);
    }



}

public class WatermelonSlice : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults()
    {
        // This code matches the ApplePie code.


        Item.DefaultToFood(36, 34, BuffID.WellFed, 5760);
        Item.value = Item.buyPrice(0, 3);
        Item.rare = ItemRarityID.Blue;
    }
}
