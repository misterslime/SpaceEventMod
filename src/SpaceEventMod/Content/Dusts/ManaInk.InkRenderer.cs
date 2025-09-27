using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Graphics;
using SpaceEventMod.Core.Utilities;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Dusts;

[Autoload(Side = ModSide.Client)]
public class InkRenderer : ModSystem
{
    public override void PostDrawTiles()
    {
        Pipeline pipeline = Graphics.BeginPipeline(0.5f);

        var inkTexture = Assets.Assets.Textures.Dusts.MediumSmoke.Value;

        foreach (var dust in Main.dust.Where(d => d.type == ModContent.DustType<ManaInk>() && d.active))
        {
            if (dust.customData == null || dust.customData is not ManaInkData manaInkData)
                return;

            var frame = new Rectangle(0, 34 * manaInkData.FrameVariant, 32, 34);

            var drawPosition = Vector2.Lerp(manaInkData.TargetPosition, dust.position, EasingFunctions.SineEaseInOut(Math.Clamp((manaInkData.Lifetime - dust.fadeIn) / 60, 0, 1)));

            pipeline.DrawSprite(
                inkTexture,
                (manaInkData.InkType == InkType.Orbiting ? drawPosition : dust.position) - Main.screenPosition,
                dust.color,
                frame,
                dust.rotation,
                frame.Size() * 0.5f,
                new Vector2(dust.scale));
        }

        pipeline.SetBlendState(BlendStates.Stencil);

        var whitePixel = Assets.Assets.Textures.WhitePixel.Value;

        foreach (var dust in Main.dust.Where(d => d.type == ModContent.DustType<InkStar>() && d.active))
        {
            if (dust.customData == null || dust.customData is not InkStarData manaInkData)
                return;

            var opacity = (float)Math.Sin((dust.fadeIn / 20) * MathHelper.Pi);

            pipeline.DrawSprite(
                whitePixel,
                dust.position - Main.screenPosition,
                manaInkData.PixelColor * opacity,
                whitePixel.Bounds,
                0f,
                whitePixel.Size() * 0.5f,
                new Vector2(2f));
        }

        pipeline
            .ApplyOutline(new Color(69, 77, 255))
            .Schedule(RenderLayer.AfterTiles);
    }
}
