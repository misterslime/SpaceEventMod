using SpaceEventMod.Content.Props;
using SpaceEventMod.Core.Props;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items;

public class Debug : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 120;
        Item.height = 80;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = 1000;
        Item.rare = ItemRarityID.Green;
    }


    public override bool? UseItem(Player player)
    {
        int asteroidType = Main.rand.Next(6);

        switch (asteroidType)
        {
            case 0:
                PropManager.NewProp(new Asteroid(Main.MouseWorld, 48, 16, "SpaceEventMod/Assets/Textures/Props/Asteroid3Small", 1));
                break;

            case 1:
                PropManager.NewProp(new Asteroid(Main.MouseWorld, 48, 32, "SpaceEventMod/Assets/Textures/Props/Asteroid3Medium", 1));
                break;

            case 2:
                PropManager.NewProp(new Asteroid(Main.MouseWorld, 48, 48, "SpaceEventMod/Assets/Textures/Props/Asteroid3Large", 1));
                break;

            case 3:
                PropManager.NewProp(new Asteroid(Main.MouseWorld, 64, 24, "SpaceEventMod/Assets/Textures/Props/Asteroid4Small", 1));
                break;

            case 4:
                PropManager.NewProp(new Asteroid(Main.MouseWorld, 64, 32, "SpaceEventMod/Assets/Textures/Props/Asteroid4Medium", 1));
                break;

            case 5:
                PropManager.NewProp(new Asteroid(Main.MouseWorld, 64, 48, "SpaceEventMod/Assets/Textures/Props/Asteroid4Large", 1));
                break;
        }

        return true;
    }
}
