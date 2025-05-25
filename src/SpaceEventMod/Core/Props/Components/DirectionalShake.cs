using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props.Systems;
using Terraria;

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

    public override void Dispose()
    {
        DirectionalShakeSystem.Unregister(this);
    }

    public void Update()
    {
        if (this.Time > 0)
            this.Time -= 1;
    }

    public float GetStrength()
    {
        return MaxStrength * ((float)Time / (float)MaxTime);
    }
}
