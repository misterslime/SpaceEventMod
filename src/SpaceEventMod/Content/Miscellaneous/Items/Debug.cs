using Microsoft.Xna.Framework;
using SpaceEventMod.Content.CellularGrowth.ConnectiveCells;
using SpaceEventMod.Content.Space;
using SpaceEventMod.Content.Space.LevelElements;
using SpaceEventMod.Core;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Items;

internal class Debug : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 120;
        Item.height = 80;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.channel = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = 1000;
        Item.rare = ItemRarityID.Green;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player)
    {

        if (player.altFunctionUse == 2)
        {
            if (!Filters.Scene["SeaDistortFog"].IsActive())
                Filters.Scene.Activate("SeaDistortFog");
            else
                Filters.Scene.Deactivate("SeaDistortFog");


            if (!SpaceEvent.Sea.Active)
                SpaceEvent.Sea = new FirmamentSea(16, 64, 3);
            else
            {
                var sea = SpaceEvent.Sea;
                sea.Despawning = sea.Despawning ? false : true;
                SpaceEvent.Sea = sea;
            }
        }
        else
        {
            var point = Main.MouseWorld.ToTileCoordinates();

            ConnectiveCellSystem.TryAddCell(point.X, point.Y);
        }

        return true;
    }
}
