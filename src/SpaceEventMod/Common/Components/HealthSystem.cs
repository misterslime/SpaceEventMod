using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;

namespace SpaceEventMod.Common.Components;

public class HealthSystem : ComponentSystem<Health>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            // delete the prop if durability is now below 0
            if (component.Current <= 0)
            {
                if (component.HasComponent<Transformation>())
                    SoundEngine.PlaySound(component.DeathSound, component.GetComponent<Transformation>().Position);

                ComponentManager.QueuePropRemoval(component.prop);
                return;
            }
        }
    }
}
