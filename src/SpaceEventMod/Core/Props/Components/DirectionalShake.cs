using Microsoft.Xna.Framework;
using System.Linq;

namespace SpaceEventMod.Core.Props.Components;

public class DirectionalShake : Component
{
    public float MaxStrength;
    public Vector2 UnitDirection;
    public int Time;
    public int MaxTime;

    public DirectionalShake()
    {
        DirectionalShakeSystem.Register(this);
    }
}

public class DirectionalShakeSystem : ComponentSystem<DirectionalShake>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components.ToList())
        {
            if (component.Time > 0)
                component.Time -= 1;
        }
    }
}
