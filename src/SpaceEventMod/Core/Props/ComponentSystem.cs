using System.Collections.Generic;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Props;

public class ComponentSystem<T> : ModSystem where T : Component
{
    protected static List<T> components = new List<T>();

    public static void Register(T component)
    {
        components.Add(component);
    }

    public static void Unregister(T component)
    {
        components.Remove(component);
    }
}

