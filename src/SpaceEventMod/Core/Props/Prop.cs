using System.Collections.Generic;

namespace SpaceEventMod.Core.Props;

public abstract class Prop
{
    public int ID { get; set; }

    public List<Component> components = new List<Component>();

    public Prop()
    {
    }

    public void AddComponent(Component component)
    {
        components.Add(component);
        component.prop = this;
    }

    public T GetComponent<T>() where T : Component
    {
        foreach (Component component in components)
        {
            if (component.GetType() == typeof(T))
                return (T)component;
        }

        return null;
    }

    public void DisposeComponents()
    {
        foreach (Component component in components)
        {
            component.Dispose();
        }
    }
}
