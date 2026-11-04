using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.Graphics;
using SpaceEventMod.Common.Tweening;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Miscellaneous.Dusts;

internal class TileSplat : ModDust
{
    private const int LIFESPAN = 600;

    public override void Load()
    {
        LightingEngine.AfterTiles += DrawSplatDusts;
    }

    public override void Unload()
    {
        LightingEngine.AfterTiles -= DrawSplatDusts;
    }


    public override bool Update(Dust dust)
    {
        if (dust.customData is not int lifespan)
        {
            dust.active = false;
            return false;
        }

        dust.fadeIn++;

        if (dust.fadeIn > lifespan)
            dust.active = false;

        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        return false;
    }

    private static void DrawSplatDusts(object? sender, DrawEventArgs e)
    {
        if (!Main.dust.Where(d => d.type == ModContent.DustType<TileSplat>()).Any())
            return;

        int splatDust = ModContent.DustType<TileSplat>();

        using (var _ = e.SpriteBatch.Scope())
        {
            var texture = Assets.Textures.Miscellaneous.Dusts.TileSplat.Asset.Value;

            var transform = Matrix.Invert(Main.GameViewMatrix.TransformationMatrix);

            // there is SO a simpler way to do this
            var scale = (new Vector2(Main.screenWidth, Main.screenHeight).Transform(transform) - Vector2.Zero.Transform(transform)) / new Vector2(Main.screenWidth, Main.screenHeight);

            var shaderPass = Assets.Shaders.Fragment.TileStencil.CreatePass1();

            shaderPass.Parameters.TileTarget = Main.instance.tileTarget;
            shaderPass.Parameters.TileTargetSize = Main.instance.tileTarget.Size();
            shaderPass.Parameters.TileTargetPosition = Main.sceneTilePos;

            shaderPass.Parameters.TileTarget2 = Main.instance.tile2Target;
            shaderPass.Parameters.TileTarget2Size = Main.instance.tile2Target.Size();
            shaderPass.Parameters.TileTarget2Position = Main.sceneTile2Pos;

            shaderPass.Parameters.ScreenPosition = Main.screenPosition;
            shaderPass.Parameters.ScreenZoomRatio = Vector2.One;

            shaderPass.Apply();

            e.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, shaderPass.Shader, Matrix.Identity);

            foreach (var dust in Main.dust)
            {
                if (!dust.active || dust.type != splatDust)
                    continue;

                if (dust.customData is not int lifespan)
                    continue;

                var color = dust.color * (1f - EasingFunctions.InCirc(dust.fadeIn / lifespan));
                e.SpriteBatch.Draw(texture, dust.position - Main.screenPosition, null, color, dust.rotation, texture.Size() * 0.5f, dust.scale, 0f, 0f);

            }

            e.SpriteBatch.End();
        }
    }
}
