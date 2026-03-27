using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.CellularGrowth.Items.Amoeba;

[AutoloadEquip(EquipType.Body)]
internal class AmoebicBreastplate : ModItem
{
    public override void SetStaticDefaults()
    {
        int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

        ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
        ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
        ArmorIDs.Body.Sets.HidesHands[equipSlotBody] = true;
    }

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
