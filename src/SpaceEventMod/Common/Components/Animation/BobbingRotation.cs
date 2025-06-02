using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core.Props;
using System;
using Terraria;

namespace SpaceEventMod.Common.Components.Animation;

public class BobbingRotation(float strength) : Component
{
    public float Strength = strength;
}
