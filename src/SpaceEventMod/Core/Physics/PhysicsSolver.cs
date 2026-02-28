using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceEventMod.Core.Physics;

internal class PhysicsSolver
{
    private readonly List<IPass> _passes = new List<IPass>();

    public void AddPhysicsPass(IPass pass) => _passes.Add(pass);

    public void RunPhysicsPasses(List<PhysicsObject> physicsObjects)
    {
        if (physicsObjects.Count <= 0)
            return;

        foreach (var pass in _passes)
        {
            for (var i = 0; i < physicsObjects.Count; i++)
            {
                if (CanRunPass(pass, physicsObjects[i]))
                    pass.Pass(physicsObjects[i]);
            }
        }
    }

    private bool CanRunPass(IPass pass, PhysicsObject physicsObject)
    {
        var conditional = pass.GetType().GetCustomAttributes(true);

        if (conditional.Length <= 0)
            return true;

        var rejects = from condition in conditional
                      where condition is RejectsAttribute
                      from type in (condition as RejectsAttribute).Types
                      select type;

        if (rejects.Any())
        {
            foreach (var component in physicsObject.Components)
            {
                if (rejects.Contains(component.GetType()))
                    return false;
            }
        }

        var needs = from condition in conditional
                    where condition is NeedsAttribute
                    from type in (condition as NeedsAttribute).Types
                    select type;

        if (needs.Any())
        {
            var componentTypes = from component in physicsObject.Components
                                 select component.GetType();

            if (!needs.All(componentTypes.Contains))
                return false;
        }

        return true;
    }
}