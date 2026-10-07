using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Common.StarsapCoating;

[Autoload(Side = ModSide.Client)]
public class StarsapTileRenderer : ILoadable
{
    public void Load(Mod mod) => LightingEngine.AfterTiles += DrawStarsap;

    public void Unload() => LightingEngine.AfterTiles -= DrawStarsap;

    private void DrawStarsap(object? sender, DrawEventArgs e)
    {
        for (var i = -2 + (int)Main.screenPosition.X / 16; i <= 2 + (int)(Main.screenPosition.X + Main.screenWidth) / 16; i++)
        {
            for (var j = -2 + (int)Main.screenPosition.Y / 16; j <= 2 + (int)(Main.screenPosition.Y + Main.screenHeight) / 16; j++)
            {
                if (WorldGen.InWorld(i, j))
                {
                    var tile = Framing.GetTileSafely(i, j);
                    ref StarsapTileData tileData = ref tile.Get<StarsapTileData>();

                    if (tileData.Coated)
                    {
                        var target = new Rectangle((int)(i * 16 - Main.screenPosition.X), (int)(j * 16 - Main.screenPosition.Y), 16, 16);
                        var tex = Assets.Textures.WhitePixel.Asset.Value;

                        e.SpriteBatch.Draw(tex, target, null, Color.Magenta);
                    }
                }
            }
        }
    }
}
