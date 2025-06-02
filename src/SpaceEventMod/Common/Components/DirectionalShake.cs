using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props;
using System;
using Terraria;

namespace SpaceEventMod.Common.Components;

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
