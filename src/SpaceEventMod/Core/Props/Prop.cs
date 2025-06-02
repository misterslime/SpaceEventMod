using System;
using System.Collections.Generic;

namespace SpaceEventMod.Core.Props;

public class Prop
{
    List<Component> components;
    Guid ID;

    /// <summary>
    /// Creates a new prop object which can be used to initialize a bunch of components at once with the same prop guid.
    /// </summary>
    public Prop()
    {
        components = new List<Component>();
        ID = Guid.NewGuid();
    }

    /// <summary>
    /// Add a component to this prop.
    /// </summary>
    /// <param name="component">Component to add.</param>
    public Prop AddComponent(Component component)
    {
        components.Add(component);
        component.prop = this.ID;
        return this;
    }

    /// <summary>
    /// Adds the prop's components to the list of components active in the world.
    /// </summary>
    public void Register()
    {
        foreach (var component in components)
        {
            ComponentManager.AddComponent(component);
        }
    }
}
