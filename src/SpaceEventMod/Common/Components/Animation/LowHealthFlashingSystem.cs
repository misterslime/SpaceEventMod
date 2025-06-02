using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core.Props;
using SpaceEventMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework;

namespace SpaceEventMod.Common.Components.Animation;

public class LowHealthFlashingSystem : ComponentSystem<LowHealthFlashing>
{
    public override void PostUpdateEverything()
    {
        foreach (var component in components)
        {
            var wave = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.1f), 2);
            var lifeRatio = component.GetComponent<Health>().Current / (float)component.GetComponent<Health>().MaxHealth;
            component.GetComponent<Sprite>().DrawColor = Color.Lerp(Color.White, component.FlashColor, wave * EasingFunctions.CircEaseIn(1 - lifeRatio));
        }
    }
}
