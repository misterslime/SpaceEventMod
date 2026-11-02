using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles.CellWood;

internal class CellWood : ModTile
{
    public void SetItemStaticDefaults(ModItem modItem)
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
        RecipeGroup.recipeGroups[RecipeGroupID.Wood].ValidItems.Add(modItem.Type);
        modItem.Item.ResearchUnlockCount = 100;
    }

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = false;
        Main.tileBrick[Type] = true;
        //Main.tileMergeDirt[Type] = true;

        DustType = DustID.t_PearlWood;
        AddMapEntry(new Color(19, 223, 210));
    }
}

internal class CellWoodItem : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
        RecipeGroup.recipeGroups[RecipeGroupID.Wood].ValidItems.Add(Type);
        Item.ResearchUnlockCount = 100;
    }
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<CellWood>(), 0);
        Item.width = 28;
        Item.height = 22;
        Item.maxStack = 9999;
        Item.value = 100;
    }
}