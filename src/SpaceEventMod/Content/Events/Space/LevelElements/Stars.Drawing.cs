using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.Space.LevelElements;

[Autoload(Side = ModSide.Client)]
public class StarDrawing : ILoadable
{
    public void Load(Mod mod) => On_Main.DrawDust += DrawStars;

    public void Unload() => On_Main.DrawDust -= DrawStars;

    private void DrawStars(On_Main.orig_DrawDust orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (var i = 0; i < Stars.List.Count; i++)
        {
            var star = Stars.List[i];

            var texture = ModContent.Request<Texture2D>(star.TexturePath).Value;
            var drawPosition = star.GetCenter() - Main.screenPosition;
            var origin = texture.Size() * 0.5f;

            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = star.Durability / 1000f;
            var drawColor = Color.Lerp(Color.White, Color.Transparent, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));

            var shakeVector = MathF.Sin(Main.GameUpdateCount) * 2f * (star.ShakeTime / 20f) * star.ShakeDirection;

            Main.EntitySpriteDraw(texture, drawPosition + star.SpriteDisplacement + shakeVector, texture.Frame(), drawColor, star.Rotation, origin, 1f, star.Effects);
        }

        Main.spriteBatch.End();

        orig(self);
    }
}
