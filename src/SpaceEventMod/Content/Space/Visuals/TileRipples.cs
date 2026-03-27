using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Content.Space.Rendering;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space.Visuals;

internal class TileRipples : ILoadable
{
    public void Load(Mod mod)
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.DrawInfernoRings += DrawRipples;
        });
    }

    public void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            On_Main.DrawInfernoRings -= DrawRipples;
        });
    }

    private void DrawRipples(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        if (self.wallTarget is null || self.tileTarget is null || self.tile2Target is null)
        {
            orig(self);
            return;
        }

        using var rippleTarget = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (width, height) => (width / 2, height / 2));
        using var rippleTarget2 = ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (width, height) => (width / 2, height / 2));

        using (rippleTarget.Target.Scope(clearColor: Color.Transparent))
        {
            using var sbScope = Main.spriteBatch.Scope();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            var changeColor = Assets.Shaders.Fragment.ChangeColor.CreateAwesomePass();

            changeColor.Parameters.color = Color.Red.ToVector4();
            changeColor.Apply();

            Main.spriteBatch.Draw(self.wallTarget, (Main.sceneWallPos - Main.screenPosition) * 0.5f, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(self.tileTarget, (Main.sceneTilePos - Main.screenPosition) * 0.5f, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(self.tile2Target, (Main.sceneTile2Pos - Main.screenPosition) * 0.5f, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();

        }

        using (rippleTarget2.Target.Scope(clearColor: Color.Transparent))
        {
            using var sbScope = Main.spriteBatch.Scope();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            var seaRipplesBlur = Assets.Shaders.Space.SeaRippleBlur.CreatePass0();

            seaRipplesBlur.Parameters.blurRadius = 0.002f;
            seaRipplesBlur.Apply();

            Main.spriteBatch.Draw(rippleTarget.Target, Vector2.Zero, null, new Color(1, 1, 1, 0), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();

        }

        using (Main.spriteBatch.Scope())
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            var seaRipples = Assets.Shaders.Space.SeaRipples.CreatePass0();

            seaRipples.Parameters.pixelSize = (Vector2.One * 2f) / rippleTarget2.Target.Size();
            seaRipples.Parameters.noise = Assets.Textures.Noise.Bubble.Asset.Value;
            seaRipples.Parameters.sea = SeaTargets.SeaRenderTarget;
            seaRipples.Parameters.uTime = Main.GlobalTimeWrappedHourly * 0.06f;
            seaRipples.Parameters.uScale = 1f;
            seaRipples.Parameters.factor = 2f;
            seaRipples.Parameters.quantization = 4;
            seaRipples.Apply();

            Main.spriteBatch.Draw(rippleTarget2.Target, Vector2.Zero, null, new Color(103, 126, 255), 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
        }

        orig(self);
    }
}
