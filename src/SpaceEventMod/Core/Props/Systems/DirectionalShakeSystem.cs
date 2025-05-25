using SpaceEventMod.Core.Props.Components;
using System.Linq;

namespace SpaceEventMod.Core.Props.Systems;

public class DirectionalShakeSystem : PropSystem<DirectionalShake>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components.ToList())
        {
            component.Update();
        }
    }
}
