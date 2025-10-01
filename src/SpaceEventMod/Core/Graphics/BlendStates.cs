using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Graphics;

internal static class BlendStates
{
    internal readonly static BlendState Stencil = new BlendState()
    {
        Name = "BlendState.Stencil",
        ColorSourceBlend = Blend.DestinationAlpha,
        AlphaSourceBlend = Blend.DestinationAlpha,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaDestinationBlend = Blend.InverseSourceAlpha,
        ColorBlendFunction = BlendFunction.Add,
        AlphaBlendFunction = BlendFunction.Add
    };

    public readonly static BlendState Subtractive = new BlendState
    {
        Name = "BlendState.Subtractive",
        ColorSourceBlend = Blend.SourceAlpha,
        AlphaSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaBlendFunction = BlendFunction.ReverseSubtract
    };

    internal readonly static BlendState Multiply = new BlendState()
    {
        Name = "BlendState.Multiply",
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.SourceColor
    };
}
