using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

internal class Cocooned : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<CocoonPlayer>().Cocooned = true;
    }
}
