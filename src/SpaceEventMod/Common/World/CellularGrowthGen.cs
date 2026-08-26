using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpaceEventMod.Common.World;

public class CellularGrowthGen : ModSystem
{
    public static Line[] _connectiveCells;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int islandsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Lakes"));

        if (islandsIndex != -1)
        {
            tasks.Insert(islandsIndex - 1, new CellularGrowthPass("Cellular Growth", 100f));
        }
    }
}