using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

[AutoloadEquip(EquipType.Legs)]
internal class AmoebicLeggings : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = Item.sellPrice(silver: 60);
        Item.rare = ItemRarityID.Green;
        Item.defense = 8;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetCritChance(DamageClass.Generic) += 5f / 100f;
    }
}
