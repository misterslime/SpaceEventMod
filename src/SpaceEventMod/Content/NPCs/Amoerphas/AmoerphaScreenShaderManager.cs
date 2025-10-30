using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Amoerphas;

internal class AmoerphaScreenShaderManager : ModSystem
{
    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            var shader = Assets.Assets.Shaders.Filters.AmoerphaBody;
            Filters.Scene["AmoerphaBalls"] = new Filter(new ScreenShaderData(shader, "Pass1"), EffectPriority.VeryHigh);
            Filters.Scene["AmoerphaBalls"].Load();
        }
    }

    public static bool Update(in RenderTarget2D metaballTarget)
    {
        if (Main.netMode == NetmodeID.Server)
            return false;

        if (!Filters.Scene["AmoerphaBalls"].IsActive())
            Filters.Scene.Activate("AmoerphaBalls");

        Filters.Scene["AmoerphaBalls"]
            .GetShader()
            .UseColor(
                0.55f,
                ((Main.MouseWorld - Main.LocalPlayer.Center).ToRotation() + MathF.PI) / MathHelper.TwoPi,
                0f)
            .UseImage(
                metaballTarget, 0,
                new SamplerState()
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp
                })
            .UseImage(
                Assets.Assets.Textures.Palettes.Amoerpha.AmoerphaColorMap.Value, 1,
                new SamplerState()
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Clamp
                })
            .UseImage(
                Assets.Assets.Textures.Palettes.Amoerpha.AmoerphaOutlineColorMap.Value, 2,
                new SamplerState()
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Clamp
                });

        return false;
    }

    public static void Deactivate()
    {
        if (Main.netMode != NetmodeID.Server && Filters.Scene["AmoerphaBalls"].IsActive())
        {
            Filters.Scene["AmoerphaBalls"].Deactivate();
        }
    }
}
