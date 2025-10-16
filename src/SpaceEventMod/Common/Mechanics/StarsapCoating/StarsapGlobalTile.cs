using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.Mechanics.StarsapCoating;

internal class StarsapGlobalTile : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        StarsapCoatingSystem.CoatTile(i, j, false);
    }
}
