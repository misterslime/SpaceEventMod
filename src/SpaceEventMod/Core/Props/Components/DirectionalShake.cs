using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class DirectionalShake : Component
{
    public float MaxStrength;
    public Vector2 UnitDirection;
    public int Time;
    public int MaxTime;
}

public class DirectionalShakeSystem : ComponentSystem<DirectionalShake>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            component.GetComponent<Sprite>().SpriteDisplacement = MathF.Sin(Main.GameUpdateCount) * component.MaxStrength * ((float)component.Time / (float)component.MaxTime) * component.UnitDirection;

            if (component.Time <= 0)
                continue;

            component.Time -= 1;
        }
    }
}
