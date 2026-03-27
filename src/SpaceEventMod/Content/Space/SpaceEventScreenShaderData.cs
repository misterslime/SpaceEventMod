using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Content.Space.Rendering;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace SpaceEventMod.Content.Space;

internal class SpaceEventScreenShaderData : ScreenShaderData
{
    private static Filter _myFilter;

    public SpaceEventScreenShaderData(Asset<Effect> shader, string passName)
            : base(shader, passName)
    {
    }

    [OnLoad]
    private static void Load()
    {
        var shader = Assets.Shaders.Space.SeaDistortFog.Asset;

        _myFilter = new Filter(new SpaceEventScreenShaderData(shader, "Pass0")
            .UseImage(Assets.Textures.Noise.SwirlyDisplaceNoise.Asset, 0, SamplerState.LinearWrap), EffectPriority.VeryHigh);

        Filters.Scene["SeaDistortFog"] = _myFilter;
        Filters.Scene["SeaDistortFog"].Load();
    }

    [ModSystemHooks.PostUpdateEverything]
    private static void UpdateShaderParameters()
    {
        if (_myFilter is null || SeaTargets.SeaRenderTarget is null)
            return;

        Filters.Scene["SeaDistortFog"]._shader.UseImage(SeaTargets.SeaRenderTarget, 1, SamplerState.LinearWrap);
    }

    public override void Apply()
    {
        // base.Shader.Parameters["fogColor"]?.SetValue(new Vector4(0.0f, 0.25f, 1.0f, 0.25f));
        base.Shader.Parameters["fogColor"]?.SetValue(new Vector4(0.0f, 0.25f, 1.0f, 0.35f));
        base.Shader.Parameters["fogStart"]?.SetValue(0.15f);
        base.Shader.Parameters["fogEnd"]?.SetValue(0.65f);
        base.Shader.Parameters["distortIntensity"]?.SetValue(0.07f);
        base.Shader.Parameters["distortNoiseScale"]?.SetValue(0.001f);
        base.Shader.Parameters["timeScale"]?.SetValue(0.02f);
        base.Shader.Parameters["blurMulti"]?.SetValue(0.0005f);

        base.Apply();
    }
}
