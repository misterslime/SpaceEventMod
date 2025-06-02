using SpaceEventMod.Core.Props;
using Terraria;

namespace SpaceEventMod.Common.Components;

public class DespawnWithDistance(float distance) : Component
{
    public float Distance = distance;
}

public class DespawnWithDistanceSystem : ComponentSystem<DespawnWithDistance>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            var shouldDespawn = (component.GetComponent<Transformation>().Position - Main.LocalPlayer.Center).LengthSquared() > component.Distance * component.Distance;


            if (shouldDespawn)
            {
                ComponentManager.QueuePropRemoval(component.prop);
            }
        }
    }
}
