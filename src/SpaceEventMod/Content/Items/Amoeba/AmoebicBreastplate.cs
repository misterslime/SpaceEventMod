using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

[AutoloadEquip(EquipType.Body)]
internal class AmoebicBreastplate : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = Item.sellPrice(silver: 70);
        Item.rare = ItemRarityID.Green;
        Item.defense = 8;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Generic) += 5f / 100f;
    }
}
