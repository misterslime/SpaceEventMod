using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TileHelper.Common;

namespace SpaceEventMod.Content.CellularGrowth.Walls;

internal class CosmostoneWall : ModWall, ILoadItem
{
    public void SetItemStaticDefaults(ModItem modItem) => modItem.Item.ResearchUnlockCount = 400;

    public override void SetStaticDefaults()
    {
        var wallColor = Color.Gray * 0.6f;
        wallColor.A = 255;

        AddMapEntry(wallColor);
    }
}
