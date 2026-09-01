using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Tiles;

internal class CosmostoneBrick : ModTile, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 100;

    public void AddItemRecipes(ModItem modItem) => modItem.CreateRecipe()
            .AddIngredient(ModContent.GetInstance<Cosmostone>().AutoItemType(), 2)
            .AddTile(TileID.Furnaces)
            .Register();

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;
        Main.tileMerge[ModContent.TileType<Cosmostone>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<Cosmostone>()] = true;
        Main.tileMerge[ModContent.TileType<HerbCell>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<HerbCell>()] = true;
        Main.tileMerge[ModContent.TileType<Cosmoss>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<Cosmoss>()] = true;
        Main.tileBlendAll[Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        AddMapEntry(new Color(62, 56, 80));

        DustType = DustID.Stone;
        HitSound = SoundID.Tink;

    }
}
