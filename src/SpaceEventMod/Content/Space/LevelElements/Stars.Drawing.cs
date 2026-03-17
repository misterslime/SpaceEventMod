using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation.Tweening;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.LevelElements;

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

            var texture = Assets.Assets.Textures.Space.LevelElements.Star.Value;
            var drawPosition = star.GetCenter() - Main.screenPosition;
            var origin = star.Frame.Center.ToVector2();

            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = star.Durability / 1000f;
            var drawColor = Color.Lerp(Color.White, Color.Transparent, wave * EasingFunctions.InCirc(1 - lifeRatio));

            var shakeVector = MathF.Sin(Main.GameUpdateCount) * 2f * (star.ShakeTime / 20f) * star.ShakeDirection;

            Main.EntitySpriteDraw(texture, drawPosition + star.SpriteDisplacement + shakeVector, star.Frame, drawColor, star.Rotation, origin, 1f, star.Effects);
        }

        Main.spriteBatch.End();

        orig(self);
    }
}
