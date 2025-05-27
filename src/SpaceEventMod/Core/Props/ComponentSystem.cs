using System.Collections.Generic;
using System.Linq;
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

    public override void PostUpdateEverything()
    {
        // make sure components that should be disposed of are disposed of
        foreach (var component in components.ToList())
        {
            if (component.Dispose)
                Unregister(component);
        }
    }

    public override void OnWorldUnload()
    {
        // make sure components that should be disposed of are disposed of
        foreach (var component in components.ToList())
        {
            Unregister(component);
        }
    }
}

