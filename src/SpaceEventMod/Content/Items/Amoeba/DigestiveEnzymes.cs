using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

internal class DigestiveEnzymes : ModBuff
{
    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense += 25;
        player.moveSpeed += 1f;
    }
}
