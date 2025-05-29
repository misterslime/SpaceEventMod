using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Props;

public class Component
{
    public Guid prop;
    public bool Dispose = false;

    public T GetComponent<T>() where T : Component
    {
        foreach (Component component in ComponentManager.components)
        {
            if (component.prop == this.prop && component.GetType() == typeof(T))
                return (T)component;
        }

        return null;
    }
}

public class ComponentSystem<T> : ModSystem where T : Component
{
    protected List<T> components => ComponentManager.GetComponents<T>();
}
