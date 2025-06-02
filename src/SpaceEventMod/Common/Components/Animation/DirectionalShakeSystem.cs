using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core.Props;
using System;
using Terraria;

namespace SpaceEventMod.Common.Components.Animation;

public class DirectionalShakeSystem : ComponentSystem<DirectionalShake>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            if (component.Time <= 0)
                continue;

            component.Time -= 1;
            component.GetComponent<Sprite>().SpriteDisplacement = MathF.Sin(Main.GameUpdateCount) * component.MaxStrength * (component.Time / (float)component.MaxTime) * component.UnitDirection;
        }
    }
}
