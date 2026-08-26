using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class CellWood : ModTile, ILoadItem
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
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;
        //Main.tileMergeDirt[Type] = true;

        DustType = DustID.t_PearlWood;
        AddMapEntry(new Color(19, 223, 210));
    }
}
