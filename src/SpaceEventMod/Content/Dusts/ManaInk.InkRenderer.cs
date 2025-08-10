using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Dusts;

[Autoload(Side = ModSide.Client)]
public class InkRenderer : ModSystem
{
    public static RenderTarget2D InkRenderTarget;

    public override void Load()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths += DrawToTarget;
            On_Main.DrawDust += DrawManaInk;

            InkRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        });
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.CheckMonoliths -= DrawToTarget;
            On_Main.DrawDust -= DrawManaInk;

            InkRenderTarget?.Dispose();
            InkRenderTarget = null;
        });
    }

    private static void DrawToTarget(On_Main.orig_CheckMonoliths orig)
    {
        if (Main.gameMenu)
        {
            orig();
            return;
        }

        if (InkRenderTarget == null || InkRenderTarget.Width != Main.screenWidth || InkRenderTarget.Height != Main.screenHeight)
        {
            InkRenderTarget?.Dispose();
            InkRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }

        Main.graphics.GraphicsDevice.SetRenderTarget(InkRenderTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        var inkTexture = Assets.Assets.Textures.Dusts.MediumSmoke.Value;

        foreach (var dust in Main.dust.Where(d => d.type == ModContent.DustType<ManaInk>() && d.active))
        {
            if (dust.customData == null || dust.customData is not ManaInkData manaInkData)
                return;

            var frame = new Rectangle(0, 34 * manaInkData.FrameVariant, 32, 34);

            var drawPosition = Vector2.Lerp(manaInkData.TargetPosition, dust.position, EasingFunctions.SineEaseInOut(Math.Clamp((manaInkData.Lifetime - dust.fadeIn) / 60, 0, 1)));

            Main.spriteBatch.Draw(inkTexture, (manaInkData.InkType == InkType.Orbiting ? drawPosition : dust.position) - Main.screenPosition, frame, dust.color, dust.rotation, frame.Size() / 2f, dust.scale, SpriteEffects.None, 0f);
        }

        Main.spriteBatch.End();

        Main.instance.GraphicsDevice.SetRenderTarget(null);

        orig();
    }

    public void DrawManaInk(On_Main.orig_DrawDust orig, Main self)
    {
        if (InkRenderTarget == null)
            return;

        PixelRenderer.Draw(null, (SpriteBatch spriteBatch) =>
        {
            var inkGlow = Assets.Assets.Textures.Dusts.MediumSmoke_Glow.Value;

            foreach (var dust in Main.dust.Where(d => d.type == ModContent.DustType<ManaInk>() && d.active))
            {
                if (dust.customData == null || dust.customData is not ManaInkData manaInkData)
                    continue;

                var frame = new Rectangle(0, 38 * manaInkData.FrameVariant, 36, 38);

                var drawColor = new Color(69, 77, 255);

                var drawPosition = Vector2.Lerp(manaInkData.TargetPosition, dust.position, EasingFunctions.SineEaseInOut(Math.Clamp((manaInkData.Lifetime - dust.fadeIn) / 60, 0, 1)));

                Main.spriteBatch.Draw(inkGlow, (manaInkData.InkType == InkType.Orbiting ? drawPosition : dust.position) - Main.screenLastPosition - dust.velocity, frame, drawColor, dust.rotation, frame.Size() * 0.5f, dust.scale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(InkRenderTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        });

        var inkStencilShader = Assets.Assets.Shaders.InkStarStencil.Value;

        inkStencilShader.Parameters["ink"].SetValue(InkRenderTarget);
        inkStencilShader.Parameters["screenSize"].SetValue(new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));

        PixelRenderer.Draw(inkStencilShader, (SpriteBatch spriteBatch) =>
        {
            var whitePixel = Assets.Assets.Textures.WhitePixel.Value;

            foreach (var dust in Main.dust.Where(d => d.type == ModContent.DustType<InkStar>() && d.active))
            {
                if (dust.customData == null || dust.customData is not InkStarData manaInkData)
                    return;

                var opacity = (float)Math.Sin((dust.fadeIn / 20) * MathHelper.Pi);

                spriteBatch.Draw(whitePixel, dust.position - Main.screenPosition, whitePixel.Bounds, manaInkData.PixelColor * opacity, 0f, whitePixel.Size() * 0.5f, 2f, SpriteEffects.None, 0f);
            }
        });

        orig(self);
    }
}
