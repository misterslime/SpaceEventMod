using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core.Props;
using System;
using Terraria;

namespace SpaceEventMod.Common.Components.Animation;

/// <summary>
/// Makes this prop's <see cref="Sprite"/> component shake in a direction.<br/>
/// Requires the <see cref="Sprite"/> component to function.
/// </summary>
/// <param name="maxStrength">Magnitude of the shaking.</param>
/// <param name="unitDirection">The <see cref="Vector2"/> direction of the shaking.</param>
/// <param name="time">How long is left for the shaking.</param>
/// <param name="maxTime">Max amount of time the shaking can last.</param>
public class DirectionalShake(float maxStrength, Vector2 unitDirection, int time, int maxTime) : Component
{
    public float MaxStrength = maxStrength;
    public Vector2 UnitDirection = unitDirection;
    public int Time = time;
    public int MaxTime = maxTime;
}
