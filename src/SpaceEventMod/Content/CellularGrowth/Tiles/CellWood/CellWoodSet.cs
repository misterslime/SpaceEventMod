using Daybreak.Common.Features.Hooks;
using Terraria.ID;
using Terraria.ModLoader;
using TileHelper.Common;
using TileHelper.Content.Tiles;
using static TileHelper.Autoloader;

namespace SpaceEventMod.Content.CellularGrowth.Tiles.CellWood.Furniture;

public class CellWoodSet : ILoadable
{
    
    public void Load(Mod mod) => ILoadItem.PostAutoloadItems += LoadCellWoodFurniture;

    private static void LoadCellWoodFurniture()
    {
        string name = typeof(CellWoodSet).Namespace + ".CellWood";


        TileHelper.ArgumentCollection arguments = AllArgs(DustID.BubbleBurst_White, new(0.75f, 0.75f, 0.95f), SoundID.Dig, distortGlow: false)
            - new BarrelTile()
            - new BenchTile();

        

        int ingredient = ModContent.ItemType<CellWoodItem>();

        LoadFurnitureSet(name, arguments, ingredient, true);
    }

    public void Unload() { }
}
