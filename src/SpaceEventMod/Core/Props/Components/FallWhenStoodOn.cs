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

    public FallWhenStoodOn()
    {
        FallWhenStoodOnSystem.Register(this);
    }
}

public class FallWhenStoodOnSystem : ComponentSystem<FallWhenStoodOn>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components.ToList())
        {
            bool stoodOn = component.prop.GetComponent<Collider>().StoodOn;
            component.prop.GetComponent<DynamicMovement>().TargetPosition = stoodOn ? component.FallPosition : component.RestingPosition;
            component.prop.GetComponent<Collider>().StoodOn = false;
        }
    }
}

