using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;

namespace SpaceEventMod.Core.Props.Components;

public class Health : Component
{
    public int Current;
    public int MaxHealth;
    public SoundStyle DeathSound;
}

public class HealthSystem : ComponentSystem<Health>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            // delete the prop if durability is now below 0
            if (component.Current <= 0)
            {
                SoundEngine.PlaySound(component.DeathSound, component.GetComponent<Transformation>().Position);
                ComponentManager.QueuePropRemoval(component.prop);
                return;
            }
        }
    }
}

