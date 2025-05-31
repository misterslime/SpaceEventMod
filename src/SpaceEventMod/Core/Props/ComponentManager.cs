using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static SpaceEventMod.Assets.Assets.Textures;

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

    public override void OnWorldUnload()
    {
        components.Clear();
    }


    public static bool HasProp(Guid guid)
    {
        foreach (Component component in components)
        {
            if (component.prop == guid)
                return true;
        }

        return false;
    }

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

    public static void QueuePropRemoval(Guid prop)
    {
        foreach (Component component in components.ToList())
        {
            if (component.prop == prop)
                component.Dispose = true;
        }
    }
}
