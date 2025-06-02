using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Components.Cosmostone;

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
