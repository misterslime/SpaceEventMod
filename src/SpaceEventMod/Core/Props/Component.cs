using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Props;

public abstract class Component
{
    /// <summary>
	/// The prop identifier for this component. It must share this with every other component of the same prop or they won't be considered such.
	/// </summary>
    public Guid prop;

    /// <summary>
	/// If set to true then the component will be removed on the next frame.<br/>
    /// Defaults to <see langword="false"/>.
	/// </summary>
    public bool Dispose = false;

    /// <summary>
	/// Gets a component of a type with the same prop Guid as this one.
	/// </summary>
	/// <returns>The first component found with the type and Guid, or <see langword="null"/> if the component doesnt exist.</returns>
    public T GetComponent<T>() where T : Component
    {
        List<Component> list = new List<Component>();

        ComponentManager.components.TryGetValue(typeof(T), out list);

        if (list == default)
            list = new List<Component>();

        foreach (Component component in list)
        {
            if (component.prop == this.prop && component.GetType() == typeof(T))
                return (T)component;
        }

        return null;
    }

    /// <summary>
	/// Checks if there is a component of a certain type with the same prop Guid.
	/// </summary>
	/// <returns><see langword="true"/> if a component exists, and returns <see langword="false"/> if it doesn't.</returns>
    public bool HasComponent<T>() where T : Component
    {
        List<Component> list = new List<Component>();

        ComponentManager.components.TryGetValue(typeof(T), out list);

        if (list == default)
            list = new List<Component>();

        foreach (Component component in list)
        {
            if (component.prop == this.prop && component.GetType() == typeof(T))
                return true;
        }

        return false;
    }
}

public abstract class ComponentSystem<T> : ModSystem where T : Component
{
    protected List<T> components => ComponentManager.GetComponents<T>();
}
