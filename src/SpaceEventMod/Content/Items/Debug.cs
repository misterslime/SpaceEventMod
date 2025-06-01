using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core.Props.Components;
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
        FastNoiseLite cellular = new FastNoiseLite();
        cellular.SetNoiseType(FastNoiseLite.NoiseType.Cellular);

        new Prop().AddComponent(new AsteroidNoiseSpawner(cellular, 0.7f, 10 * 16)).Register();

        return true;
    }
}
