using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items.Amoeba;

internal class DigestiveEnzymes : ModBuff
{
    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense += 25;
        player.moveSpeed += 1f;
    }
}
