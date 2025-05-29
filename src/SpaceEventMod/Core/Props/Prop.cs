using System;
using System.Collections.Generic;
using Terraria;

namespace SpaceEventMod.Core.Props;

public class Prop
{
    List<Component> components;
    Guid ID;

    public Prop()
    {
        components = new List<Component>();
        ID = Guid.NewGuid();
    }

    /// <summary>
    /// Add a component to this prop
    /// </summary>
    public Prop AddComponent(Component component)
    {
        components.Add(component);
        component.prop = this.ID;
        return this;
    }

    /// <summary>
    /// Adds the prop's components to the list of active components
    /// </summary>
    public void Register()
    {
        ComponentManager.components.AddRange(components);
    }
}
