using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

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
            bool shouldDespawn = (component.GetComponent<Transformation>().Position - Main.LocalPlayer.Center).LengthSquared() > component.Distance * component.Distance;
            

            if (shouldDespawn)
            {
                ComponentManager.QueuePropRemoval(component.prop);
            }
        }
    }
}
