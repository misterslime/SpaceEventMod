using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Props;

public class PropManager : ModSystem
{
    public static List<Prop> props = new List<Prop>();

    public override void OnWorldUnload()
    {
        foreach (Prop prop in props)
            prop.DisposeComponents();

        props.Clear();
    }

    public static void NewProp(Prop prop)
    {
        if (props.Where(p => p.ID == prop.ID).Any())
            return;

        props.Add(prop);
    }

    public static void RemoveProp(Prop prop)
    {
        prop.DisposeComponents();
        props.Remove(prop);
    }
}
