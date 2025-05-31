using Terraria.Audio;

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

