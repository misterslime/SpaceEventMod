using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class DynamicMovement : Component
{
    public Vector2Dynamics secondOrderSolver;
    public Vector2 TargetPosition;
}

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
