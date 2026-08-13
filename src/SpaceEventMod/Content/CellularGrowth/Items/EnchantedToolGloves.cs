using Microsoft.Xna.Framework;
using SpaceEventMod.Content.Miscellaneous.Dusts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.Player;

namespace SpaceEventMod.Content.CellularGrowth.Items;

[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
internal class EnchantedToolGloves : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 46;

        Item.accessory = true;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.buyPrice(gold: 1); // Equivalent to Item.buyPrice(0, 1, 0, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (player.statMana >= 1)
        {
            player.pickSpeed -= 0.7f;
            player.GetModPlayer<EnchantedToolGlovesPlayer>().DoManaDrain = true;

            if (!hideVisual)
            {
                // mist effect
                player.GetModPlayer<EnchantedToolGlovesPlayer>().DoMistVisual = true;
            }
        }
    }

    public override void UpdateVanity(Player player)
    {
        // mist effect
        player.GetModPlayer<EnchantedToolGlovesPlayer>().DoMistVisual = true;
    }
}

internal class EnchantedToolGlovesPlayer : ModPlayer
{
    private int _frameCounter;

    public bool DoMistVisual { get; set; } = false;
    public bool DoManaDrain { get; set; } = false;

    public override void PreUpdateBuffs()
    {
        var item = Player.inventory[Player.selectedItem];

        var specialToolUsageSettings = default(SpecialToolUsageSettings);
        if (item.type == ItemID.GravediggerShovel)
        {
            var specialToolUsageSettings2 = default(SpecialToolUsageSettings);
            specialToolUsageSettings2.IsAValidTool = true;
            specialToolUsageSettings2.UsageAction = Player.UseShovel;
            specialToolUsageSettings = specialToolUsageSettings2;
        }

        var notValidTool = item.pick <= 0 && item.axe <= 0 && item.hammer <= 0 && !specialToolUsageSettings.IsAValidTool;

        if (Player.toolTime == 0 || notValidTool)
        {
            DoMistVisual = false;
            DoManaDrain = false;
            return;
        }

        if (Player.toolTime > 0)
        {
            if (DoManaDrain && _frameCounter++ % 5 == 0)
            {
                Player.CheckMana(1, true, false);
                Player.manaRegenDelay = Player.maxRegenDelay;
            }

            if (DoMistVisual && Main.rand.NextBool())
            {
                var smokeSize = Main.rand.NextFloat(0.9f, 1);
                var gushDirection = Vector2.UnitX * -Player.direction;
                gushDirection = gushDirection.RotatedByRandom(0.6f);

                var mistCenter = Player.GetFrontHandPosition(Player.compositeFrontArm.stretch, Player.compositeFrontArm.rotation);
                mistCenter -= Player.MountedCenter;
                mistCenter = Player.MountedCenter - mistCenter;

                var velocity = gushDirection * Main.rand.NextFloat(0.45f, 0.9f);

                var mist = Dust.NewDustPerfect(mistCenter, ModContent.DustType<Mist>(), velocity);
                mist.scale = smokeSize;
                mist.customData = new MistData(Main.rand.Next(3), 0.02f);
                mist.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }
        }
    }
}

