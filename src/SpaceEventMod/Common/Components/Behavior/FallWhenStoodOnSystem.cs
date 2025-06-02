using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Components.Behavior;

public class FallWhenStoodOnSystem : ComponentSystem<FallWhenStoodOn>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            var stoodOn = component.GetComponent<Collider>().StoodOn;
            component.GetComponent<DynamicMovement>().TargetPosition = stoodOn ? component.FallPosition : component.RestingPosition;
            component.GetComponent<Collider>().StoodOn = false;
        }
    }
}
