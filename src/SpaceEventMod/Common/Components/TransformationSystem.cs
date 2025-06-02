using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Components;

public class TransformationSystem : ComponentSystem<Transformation>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            component.Position += component.Velocity;
        }
    }
}
