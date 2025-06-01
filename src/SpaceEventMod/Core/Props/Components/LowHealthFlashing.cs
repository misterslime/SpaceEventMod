using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

/// <summary>
/// Makes this prop flash a color when its health is low.<br/>
/// Requires the <see cref="Sprite"/> and <see cref="Health"/> components to function.
/// </summary>
/// <param name="flashColor">Whether the collider is being stood on.</param>
public class LowHealthFlashing(Color flashColor) : Component
{
    public Color FlashColor = flashColor;
}

public class LowHealthFlashingSystem : ComponentSystem<LowHealthFlashing>
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

