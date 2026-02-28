using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

[AutoloadEquip(EquipType.Head)]
internal class AmoebicHelmet : ModItem
{
    public static LocalizedText SetBonusText { get; private set; }

    public override void SetStaticDefaults()
    {
        SetBonusText = this.GetLocalization("SetBonus");
    }


    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = Item.sellPrice(silver: 60);
        Item.rare = ItemRarityID.Green;
        Item.defense = 7;
        Item.lifeRegen = 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<AmoebicBreastplate>() && legs.type == ModContent.ItemType<AmoebicLeggings>();
    }

    public override void UpdateArmorSet(Player player)
    {
        var cocoonPlayer = player.GetModPlayer<CocoonPlayer>();

        player.setBonus = SetBonusText.Value;
        cocoonPlayer.AmoebicSet = true;

        if (cocoonPlayer.Cocooned && player.whoAmI == Main.myPlayer)
        {
            player.controlJump = false;
            player.controlDown = false;
            player.controlLeft = false;
            player.controlRight = false;
            player.controlUp = false;
            player.controlUseItem = false;
            player.controlUseTile = false;
            player.controlThrow = false;
            player.pulley = false;

            if (player.mount.Active)
            {
                player.mount.Dismount(player);
            }
        }
    }
}
