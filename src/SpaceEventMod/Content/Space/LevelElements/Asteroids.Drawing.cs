using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Animation.Tweening;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.LevelElements;

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
            Assets.Textures.Space.LevelElements.Asteroid3Small.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid3Medium.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid3Large.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid4Small.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid4Medium.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid4Large.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid6.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid9.Asset.Value,
            Assets.Textures.Space.LevelElements.Asteroid11.Asset.Value
        ];

        return textures[variant];
    }
}
