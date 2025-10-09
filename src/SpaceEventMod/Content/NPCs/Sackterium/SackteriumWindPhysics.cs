using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Content.NPCs.Sackterium;

internal class SackteriumWindPhysics : ModSystem
{
    private Vector2 GetVelocityFromWind(Vector2 entityVelocity, Rectangle rectangle)
    {
        var sackteriums = from sackterium in Main.npc
                          where sackterium.active
                          where sackterium.type == ModContent.NPCType<Sackterium>()
                          select sackterium.ModNPC as Sackterium;

        foreach (var sackterium in sackteriums)
        {
            if (!sackterium.WindGustTrigger.Intersects(rectangle))
                continue;

            sackterium.IsPushing = true;

            var windAcceleration = Vector2.UnitX.RotatedBy(sackterium.NPC.rotation);

            entityVelocity.X += windAcceleration.X;
            entityVelocity.Y += windAcceleration.Y;
        }

        return entityVelocity;
    }

    public override void PreUpdateNPCs()
    {
        foreach (var npc in Main.ActiveNPCs)
            npc.velocity = GetVelocityFromWind(npc.velocity, npc.getRect());
    }

    public override void PreUpdatePlayers()
    {
        foreach (var player in Main.ActivePlayers)
            player.velocity = GetVelocityFromWind(player.velocity, player.getRect());
    }
}
