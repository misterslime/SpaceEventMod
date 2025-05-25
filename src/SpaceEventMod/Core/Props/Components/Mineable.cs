using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace SpaceEventMod.Core.Props.Components;

public class Mineable : Component
{
    public int Durability;

    public Mineable()
    {
        MiningSystem.Register(this);
    }

    public override void Dispose()
    {
        MiningSystem.Unregister(this);
    }

    public bool IsHitting(float x, float y)
    {
        return prop.GetComponent<Hitbox>().GetBoundingBox().Contains((int)x, (int)y);
    }

    public void OnHit(Player player, Item item)
    {
        this.Durability -= item.pick;
        player.ApplyItemTime(item, player.pickSpeed * 1.5f);

        // shake when mining
        Vector2 propPosition = prop.GetComponent<Hitbox>().GetCenter();
        prop.GetComponent<DirectionalShake>().UnitDirection = propPosition - player.Center;
        prop.GetComponent<DirectionalShake>().UnitDirection.Normalize();
        prop.GetComponent<DirectionalShake>().Time = 30;

        // delete the prop if durability is now below 0
        if (Durability <= 0)
        {
            SoundEngine.PlaySound(SoundID.Item70, propPosition);
            PropManager.RemoveProp(prop);
            return;
        }

        if (Main.myPlayer == player.whoAmI)
            SoundEngine.PlaySound(SoundID.Tink, Main.MouseWorld);
    }
}
