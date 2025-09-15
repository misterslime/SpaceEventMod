using SpaceEventMod.Content.Events.Space;
using SpaceEventMod.Content.Events.Space.LevelElements;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items;

public class Debug : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 120;
        Item.height = 80;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = 1000;
        Item.rare = ItemRarityID.Green;
    }

    public override bool? UseItem(Player player)
    {
        if (!SpaceEvent.Sea.Active)
            SpaceEvent.Sea = new FirmamentSea(16, 64, 3);
        else
        {
            var sea = SpaceEvent.Sea;
            sea.Despawning = sea.Despawning ? false : true;
            SpaceEvent.Sea = sea;
        }

        return true;
    }
}
