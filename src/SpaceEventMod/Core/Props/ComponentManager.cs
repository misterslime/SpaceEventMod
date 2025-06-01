using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Props;

public class ComponentManager : ModSystem
{
    public static List<Component> components = new List<Component>();

    public override void PreUpdateEntities()
    {
        // make sure components that should be disposed of are disposed of
        foreach (var component in components.ToList())
        {
            if (component.Dispose)
                components.Remove(component);
        }
    }

    // if u leave the world the props shoudln't exist anymore
    // to-do: component that saves props
    public override void OnWorldUnload()
    {
        components.Clear();
    }


    /// <summary>
	/// Checks if there are any components with the specified prop guid
	/// </summary>
    /// <param name="guid">Guid to search for.</param>
	/// <returns><see langword="true"/> if a component with the guid exists, and returns <see langword="false"/> if none do.</returns>
    public static bool HasProp(Guid guid)
    {
        foreach (Component component in components)
        {
            if (component.prop == guid)
                return true;
        }

        return false;
    }

    /// <summary>
	/// Gets a list of every component of the specified type.
	/// </summary>
	/// <returns>Returns a list containing every component of that type, returning an empty list if none do.</returns>
    public static List<T> GetComponents<T>() where T : Component
    {
        List<T> list = new List<T>();

        foreach (Component component in components)
        {
            if (component.GetType() == typeof(T))
                list.Add((T)component);
        }

        return list;
    }

    /// <summary>
	/// Queues every component with a prop id of <paramref name="prop"/> for disposal at the next frame.
	/// </summary>
	/// <param name="prop">The prop guid to target.</param>
    public static void QueuePropRemoval(Guid prop)
    {
        foreach (Component component in components.ToList())
        {
            if (component.prop == prop)
                component.Dispose = true;
        }
    }
}
