using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core;
using SpaceEventMod.Core.Props;
using System;
using Terraria;

namespace SpaceEventMod.Common.Components.Animation;

/// <summary>
/// Makes this prop flash a color when its health is low.<br/>
/// Requires the <see cref="Sprite"/> and <see cref="Health"/> components to function.
/// </summary>
/// <param name="flashColor">Whether the collider is being stood on.</param>
public class LowHealthFlashing(Color flashColor) : Component
{
    public Color FlashColor = flashColor;
}

