using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Components.Behavior;

public class DynamicMovementSystem : ComponentSystem<DynamicMovement>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            component.GetComponent<Transformation>().Position = component.secondOrderSolver.Update(1, component.TargetPosition);
        }
    }
}
