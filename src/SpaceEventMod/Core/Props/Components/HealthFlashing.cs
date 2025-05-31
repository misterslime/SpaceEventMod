using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class HealthFlashing : Component
{
    public Color FlashColor;
}

public class HealthFlashingSystem : ComponentSystem<HealthFlashing>
{
    public override void PostUpdateEverything()
    {
        foreach (var component in components)
        {
            float wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            float lifeRatio = component.GetComponent<Health>().Current / (float)component.GetComponent<Health>().MaxHealth;
            component.GetComponent<Sprite>().DrawColor = Color.Lerp(Color.White, component.FlashColor, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));
        }
    }
}

