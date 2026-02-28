using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Items;

internal class StonePolisher : ModItem
{
    public override string Texture => "SpaceEventMod/Assets/Textures/Items/Debug";

    private Point _lastHit = Point.Zero;

    public override void SetDefaults()
    {
        Item.width = 120;
        Item.height = 80;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.autoReuse = true;
        Item.channel = true;
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.knockBack = 5f;
        Item.value = 1000;
        Item.rare = ItemRarityID.Green;
    }

    public override bool? UseItem(Player player)
    {
        var mousePos = (Main.MouseWorld / 16).ToPoint();
        Item.autoReuse = true;
        var convert = new Dictionary<ushort, ushort>
        {
            { TileID.Stone, TileID.GrayBrick },
            { TileID.GrayBrick, TileID.StoneSlab },
            { TileID.StoneSlab, TileID.Stone },

            { TileID.ShimmerBlock, TileID.ShimmerBrick },
            { TileID.ShimmerBrick, TileID.ShimmerBlock },
            { TileID.IceBlock, TileID.IceBrick },
            { TileID.IceBrick, TileID.IceBlock },
            { TileID.Ash, TileID.IridescentBrick },
            { TileID.IridescentBrick, TileID.Ash },
            { TileID.Obsidian, TileID.ObsidianBrick },
            { TileID.ObsidianBrick, TileID.Obsidian },

            { TileID.Pearlstone, TileID.PearlstoneBrick },
            { TileID.PearlstoneBrick, TileID.Pearlstone },
            { TileID.Crimstone, TileID.CrimstoneBrick },
            { TileID.CrimstoneBrick, TileID.FleshBlock },
            { TileID.FleshBlock, TileID.Crimstone },
            { TileID.Ebonstone, TileID.EbonstoneBrick },
            { TileID.EbonstoneBrick, TileID.LesionBlock },
            { TileID.LesionBlock, TileID.Ebonstone },

            { TileID.BlueDungeonBrick, TileID.CrackedBlueDungeonBrick },
            { TileID.CrackedBlueDungeonBrick, TileID.BlueDungeonBrick },
            { TileID.GreenDungeonBrick, TileID.CrackedGreenDungeonBrick },
            { TileID.CrackedGreenDungeonBrick, TileID.GreenDungeonBrick },
            { TileID.PinkDungeonBrick, TileID.CrackedPinkDungeonBrick },
            { TileID.CrackedPinkDungeonBrick, TileID.PinkDungeonBrick },

            { TileID.Sandstone, TileID.SmoothSandstone },
            { TileID.SmoothSandstone, TileID.Sandstone },
            { TileID.Granite, TileID.GraniteBlock },
            { TileID.GraniteBlock, TileID.Granite },
            { TileID.Marble, TileID.MarbleBlock },
            { TileID.MarbleBlock, TileID.Marble },
        };

        if (_lastHit != mousePos && Main.tile[mousePos].HasTile && convert.ContainsKey(Main.tile[mousePos].TileType))
        {
            WorldGen.KillTile(mousePos.X, mousePos.Y, true, false);
            Main.tile[mousePos].TileType = convert[Main.tile[mousePos].TileType];
            WorldGen.SquareTileFrame(mousePos.X, mousePos.Y);

            _lastHit = mousePos;
            return true;
        }

        return false;
    }
}
