using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.Physics.Passes.NPCs;

[Needs(typeof(NPCReference))]
internal class AnchorNPC : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        NPC npc = Main.npc[physicsObject.GetComponent<NPCReference>().NPCIndex];

        npc.Center = physicsObject.Center.Position;
    }
}
