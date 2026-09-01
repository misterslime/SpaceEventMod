using SpaceEventMod.Content.CellularGrowth.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class CosmossSeeds : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = Item.height = 20;
        Item.useStyle = 1;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.consumable = true;
        Item.value = Item.sellPrice(copper: 6);
    }

    public override void HoldItem(Player player)
    {
        Tile selected = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

        if (player.IsTargetTileInItemRange(Item) && selected.HasTile && 
            (selected.type == ModContent.TileType<Cosmostone>() || selected.type == ModContent.TileType<Cosmoss>()))
        {
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = Type;
        }
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer != player.whoAmI || !player.ItemAnimationJustStarted)
            return null;

        Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

        if (!tile.HasTile || tile.TileType != ModContent.TileType<Cosmostone>() || !player.IsTargetTileInItemRange(Item))
            return null;

        WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, ModContent.TileType<Cosmoss>(), forced: true);

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(player.whoAmI, Player.tileTargetX, Player.tileTargetY);

        return true;
    }
}
