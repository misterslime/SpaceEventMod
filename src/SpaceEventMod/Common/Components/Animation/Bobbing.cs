using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Components.Behavior;
using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core.Props;
using System;
using Terraria;

namespace SpaceEventMod.Common.Components.Animation;

public class Bobbing(float strength) : Component
{
    public float Strength = strength;
    public int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);
}