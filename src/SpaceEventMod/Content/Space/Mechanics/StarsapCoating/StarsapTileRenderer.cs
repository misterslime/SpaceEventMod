using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Mechanics.StarsapCoating;

[Autoload(Side = ModSide.Client)]
public class StarsapTileRenderer : ILoadable
{
    public void Load(Mod mod) => On_Main.DrawInfernoRings += DrawStarsap;

    public void Unload() => On_Main.DrawInfernoRings -= DrawStarsap;

    private void DrawStarsap(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

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
                        var tex = Assets.Assets.Textures.WhitePixel.Value;

                        Main.spriteBatch.Draw(tex, target, null, Color.Magenta);
                    }
                }
            }
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }
}
