using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpaceEventMod.Content.CellularGrowth.Items;

internal class WindBag : ModItem
{
    private float _containedWind;

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 44;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(0, 0, 40);

        Item.useAnimation = 25;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item1;
    }

    public override void HoldItem(Player player)
    {
        Main.NewText("Player: " + player.velocity.X);
        Main.NewText("Wind: " + Main.windSpeedCurrent);
        Main.NewText("Target: " + Main.windSpeedTarget);
        Main.NewText("Bagged: " + _containedWind);

        if (MathF.Abs(Main.windSpeedTarget) <= 0.01f || MathF.Abs(Main.windSpeedCurrent) <= 0.01f)
            return;

        if (MathF.Sign(player.velocity.X) == MathF.Sign(Main.windSpeedTarget))
            return;

        Main.windSpeedCurrent += player.velocity.X * 0.001f;
        Main.windSpeedTarget += player.velocity.X * 0.001f;

        _containedWind += MathF.Abs(player.velocity.X * 0.001f);
    }

    public override bool? UseItem(Player player)
    {
        float windSpeed = MathF.Min(_containedWind, 0.8f);

        Main.windSpeedCurrent += windSpeed * player.direction;
        Main.windSpeedTarget += windSpeed * player.direction;

        _containedWind -= windSpeed;

        return base.UseItem(player);
    }

    public override void SaveData(TagCompound tag)
    {
        tag["wind"] = _containedWind;
    }

    public override void LoadData(TagCompound tag)
    {
        _containedWind = tag.GetFloat("wind");
    }
}
