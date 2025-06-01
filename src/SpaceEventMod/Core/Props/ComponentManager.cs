using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Props;

public class ComponentManager : ModSystem
{

    public static Dictionary<Type, List<Component>> components = new Dictionary<Type, List<Component>>();

    public override void PreUpdateEntities()
    {
        // make sure components that should be disposed of are disposed of
        foreach (var componentList in components)
        {
            foreach (var component in componentList.Value.ToList())
            {
                if (component.Dispose)
                    componentList.Value.Remove(component);
            }
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
    public static bool ComponentExists<T>(Guid guid) where T : Component
    {
        List<Component> list = new List<Component>();

        components.TryGetValue(typeof(T), out list);

        if (list == default)
            list = new List<Component>();

        foreach (Component component in list)
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
        List<Component> list = new List<Component>();
        List<T> componentsList = new List<T>();

        components.TryGetValue(typeof(T), out list);

        if (list == default)
            list = new List<Component>();

        foreach (Component component in list)
        {
            componentsList.Add((T)component);
        }

        return componentsList;
    }

    public static void AddComponent(Component component)
    {
        if (components.ContainsKey(component.GetType()))
        {
            components[component.GetType()].Add(component);
        }
        else
        {
            List<Component> newComponentList = new List<Component>();
            newComponentList.Add(component);

            components.Add(component.GetType(), newComponentList);
        }
    }

    /// <summary>
	/// Queues every component with a prop id of <paramref name="prop"/> for disposal at the next frame.
	/// </summary>
	/// <param name="prop">The prop guid to target.</param>
    public static void QueuePropRemoval(Guid prop)
    {
        foreach (var componentList in components)
        {
            foreach (var component in componentList.Value.ToList())
            {
                if (component.prop == prop)
                    component.Dispose = true;
            }
        }
    }
}
