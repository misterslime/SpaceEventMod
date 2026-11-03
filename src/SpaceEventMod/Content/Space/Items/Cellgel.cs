using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Items;

internal class Cellgel : ModItem
{
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 7));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;

        Item.ResearchUnlockCount = 99;

    }

    public override void SetDefaults()
    {
        Item.width = 14;
        Item.height = 30;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Blue;
    }
}
