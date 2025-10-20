using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation.Tweening;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Events.Space.LevelElements;

[Autoload(Side = ModSide.Client)]
public class AsteroidDrawing : ILoadable
{
    public void Load(Mod mod) => On_Main.DrawDust += DrawAsteroids;

    public void Unload() => On_Main.DrawDust -= DrawAsteroids;

    private void DrawAsteroids(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        for (var i = 0; i < Asteroids.List.Count; i++)
        {
            var asteroid = Asteroids.List[i];

            var texture = GetVariantTexture(asteroid.Variant);

            var drawPosition = asteroid.GetCenter() - Main.screenPosition;
            var origin = texture.Size() * 0.5f;

            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = asteroid.Durability / 200f;
            var drawColor = Color.Lerp(Color.White, Color.Red, wave * EasingFunctions.InCirc(1 - lifeRatio));

            var shakeVector = MathF.Sin(Main.GameUpdateCount) * 2f * (asteroid.ShakeTime / 20f) * asteroid.ShakeDirection;

            Main.EntitySpriteDraw(texture, drawPosition + asteroid.SpriteDisplacement + shakeVector, texture.Frame(), drawColor, 0f, origin, 1f, asteroid.Effects);
        }

        Main.spriteBatch.End();
    }

    private Texture2D GetVariantTexture(int variant)
    {
        Texture2D[] textures = [
            Assets.Assets.Textures.Props.Asteroid3Small.Value,
            Assets.Assets.Textures.Props.Asteroid3Medium.Value,
            Assets.Assets.Textures.Props.Asteroid3Large.Value,
            Assets.Assets.Textures.Props.Asteroid4Small.Value,
            Assets.Assets.Textures.Props.Asteroid4Medium.Value,
            Assets.Assets.Textures.Props.Asteroid4Large.Value,
        ];

        return textures[variant];
    }
}
