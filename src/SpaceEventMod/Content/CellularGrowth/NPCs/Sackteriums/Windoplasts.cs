using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.NPCs.Sackteriums;

internal class Windoplasts : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);
    }
}
