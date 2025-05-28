using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class HealthFlashing : Component
{
    public Color FlashColor;

    public HealthFlashing()
    {
        HealthFlashingSystem.Register(this);
    }
}

public class HealthFlashingSystem : ComponentSystem<HealthFlashing>
{
    public override void PostUpdateEverything()
    {
        foreach (var component in components.ToList())
        {
            float wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            float lifeRatio = component.prop.GetComponent<Health>().Current / (float)component.prop.GetComponent<Health>().MaxHealth;
            component.prop.GetComponent<Sprite>().DrawColor = Color.Lerp(Color.White, component.FlashColor, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));
        }
    }
}

