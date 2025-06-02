using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Components.Animation;

public class BobbingRotationSystem : ComponentSystem<BobbingRotation>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            if (!component.HasComponent<Bobbing>())
                continue;

            var bobbing = component.GetComponent<Bobbing>();

            component.GetComponent<Sprite>().Rotation = MathF.Sin((Main.GameUpdateCount + bobbing.RandomTimeDisplacement) / 120f) * (MathF.PI / 180f) * component.Strength;
        }
    }
}
