using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props.Components;
using System.Linq;
using Terraria;

namespace SpaceEventMod.Core.Props.Systems;

public class TransformationSystem : PropSystem<Transformation>
{
    public override void PostUpdateNPCs()
    {
     

        foreach (var component in components.ToList())
        {
            component.Update();
        }
    }
}
