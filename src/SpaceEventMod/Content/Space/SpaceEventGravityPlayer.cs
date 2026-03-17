using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.Space;

public class SpaceEventGravityPlayer : ModPlayer
{
    public override void PostUpdateBuffs()
    {
        if (SpaceEvent.Sea.Active && Player.Center.Y < SpaceEvent.Sea.SeaPos.Height.Position)
            Player.gravity = 0.25f;
    }
}