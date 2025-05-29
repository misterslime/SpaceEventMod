using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class FallWhenStoodOn : Component
{
    public Vector2 RestingPosition;
    public Vector2 FallPosition;
}

public class FallWhenStoodOnSystem : ComponentSystem<FallWhenStoodOn>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            bool stoodOn = component.GetComponent<Collider>().StoodOn;
            component.GetComponent<DynamicMovement>().TargetPosition = stoodOn ? component.FallPosition : component.RestingPosition;
            component.GetComponent<Collider>().StoodOn = false;
        }
    }
}

