using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Props.Components;

public class Bobbing(float strength) : Component
{
    public float Strength = strength;
    public int RandomTimeDisplacement = Main.rand.Next(-99999, 99999);
}

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
