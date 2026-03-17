using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Walls;

internal class CosmostoneWall : ModWall
{
    public override void SetStaticDefaults()
    {
        var wallColor = Color.Gray * 0.6f;
        wallColor.A = 255;

        AddMapEntry(wallColor);
    }
}
