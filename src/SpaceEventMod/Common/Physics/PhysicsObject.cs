using SpaceEventMod.Common.Physics.Attributes;
using SpaceEventMod.Common.Physics.Components;
using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using static Terraria.Localization.NetworkText;

namespace SpaceEventMod.Common.Physics;

internal class PhysicsObject(PhysicsPoint position)
{
    private readonly List<IComponent> _components = new List<IComponent>();

    public PhysicsPoint Center { get; set; } = position;
    public List<IComponent> Components { get => _components; }

    public void AddComponent<T>(T component) where T : struct, IComponent
    {
        if (CanAddComponent<T>())
            _components.Add(component);
    }

    public bool HasComponent<T>() where T : struct, IComponent => (from component in _components
                                                                   where component is T
                                                                   select component).Any();

    public IEnumerable<T> GetInstancedComponents<T>() where T : struct, IComponent, IInstancedComponent => (from component in _components
                                                                                                            where component is T
                                                                                                            select (T)component);

    public T GetInstancedComponent<T>(int index) where T : struct, IComponent, IInstancedComponent => (T)(from component in _components
                                                                                                          where component is T
                                                                                                          select component).ElementAt(index);

    public T GetComponent<T>() where T : struct, IComponent
    {
        if (typeof(T) is IInstancedComponent)
            throw new InvalidTypeParameterException("Tried to run GetComponent with an instanced component type.");

        return (T)(from component in _components
                   where component is T
                   select component).First();
    }

    public void RemoveComponent<T>() where T : struct, IComponent
    {
        if (typeof(T) is IInstancedComponent)
            throw new InvalidTypeParameterException("Tried to run RemoveComponent with an instanced component type.");

        _components.Remove(GetComponent<T>());
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
            if (_components.Any((component) => rejects.Contains(component.GetType())))
                return false;
        }

        var needs = from condition in conditional
                    where condition is NeedsAttribute
                    from type in (condition as NeedsAttribute).Types
                    select type;

        if (needs.Any())
        {
            var componentTypes = from component in _components
                                 select component.GetType();

            if (!needs.All(componentTypes.Contains))
                return false;
        }

        return true;
    }
}
