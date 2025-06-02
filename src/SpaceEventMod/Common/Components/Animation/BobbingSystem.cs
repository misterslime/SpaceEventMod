using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Components.Behavior;
using SpaceEventMod.Common.Components.Rendering;
using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Components.Animation;

public class BobbingSystem : ComponentSystem<Bobbing>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            component.GetComponent<Sprite>().SpriteDisplacement = Vector2.Zero;
            if (component.HasComponent<Collider>() && component.GetComponent<Collider>().StoodOn)
                continue;

            component.GetComponent<Sprite>().SpriteDisplacement = MathF.Sin((Main.GameUpdateCount + component.RandomTimeDisplacement) / 60f) * component.Strength * Vector2.UnitY;
        }
    }
}

