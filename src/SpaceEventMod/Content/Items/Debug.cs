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
        PropManager.NewProp(new Cosmostone(Main.MouseWorld, 1));
        return true;
    }
}
