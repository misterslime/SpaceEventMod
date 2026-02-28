using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using SpaceEventMod.Core.Utilities.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEventMod.Core.Physics;

internal class PhysicsObject(PhysicsPoint position)
{
    public PhysicsPoint Center { get; set; } = position;
    public List<IComponent> Components { get; } = new List<IComponent>();

    public void AddComponent<T>(T component) where T : struct, IComponent
    {
        if (CanAddComponent<T>())
            Components.Add(component);
    }

    public bool HasComponent<T>() where T : struct, IComponent => (from component in Components
                                                                   where component is T
                                                                   select component).Any();

    public IEnumerable<T> GetInstancedComponents<T>() where T : struct, IComponent, IInstancedComponent => (from component in Components
                                                                                                            where component is T
                                                                                                            select (T)component);

    public T GetInstancedComponent<T>(int index) where T : struct, IComponent, IInstancedComponent => (T)(from component in Components
                                                                                                          where component is T
                                                                                                          select component).ElementAt(index);

    public T GetComponent<T>() where T : struct, IComponent
    {
        if (typeof(T) is IInstancedComponent)
            throw new InvalidTypeParameterException("Tried to run GetComponent with an instanced component type.");

        return (T)(from component in Components
                   where component is T
                   select component).First();
    }

    public void RemoveComponent<T>() where T : struct, IComponent
    {
        if (typeof(T) is IInstancedComponent)
            throw new InvalidTypeParameterException("Tried to run RemoveComponent with an instanced component type.");

        Components.Remove(GetComponent<T>());
    }

    public void AddChild(PhysicsObject child)
    {
        this.AddComponent(new ChildObject(child));
        child.AddComponent(new ParentObject(this));
    }

    private bool CanAddComponent<T>()
    {
        var conditional = typeof(T).GetCustomAttributes(true);

        if (conditional.Length <= 0)
            return true;

        var rejects = from condition in conditional
                      where condition is RejectsAttribute
                      from type in (condition as RejectsAttribute).Types
                      select type;

        if (rejects.Any())
        {
            if (Components.Any((component) => rejects.Contains(component.GetType())))
                return false;
        }

        var needs = from condition in conditional
                    where condition is NeedsAttribute
                    from type in (condition as NeedsAttribute).Types
                    select type;

        if (needs.Any())
        {
            var componentTypes = from component in Components
                                 select component.GetType();

            if (!needs.All(componentTypes.Contains))
                return false;
        }

        return true;
    }
}
