using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items.Amoeba;

internal class CocoonCooldown : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
    }
}
