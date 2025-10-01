using SpaceEventMod.Core.Physics.Attributes;
using SpaceEventMod.Core.Physics.Components;
using SpaceEventMod.Core.Physics.Interfaces;
using Terraria;

namespace SpaceEventMod.Core.Physics.Passes.NPCs;

[Needs(typeof(PhysicsShape), typeof(NPCReference))]
internal class TrailNPC : IPass
{
    public int Steps { get; init; } = 1;

    public void Pass(PhysicsObject physicsObject)
    {
        var shape = physicsObject.GetComponent<PhysicsShape>();
        var npcIndex = physicsObject.GetComponent<NPCReference>().NPCIndex;
        var npc = Main.npc[npcIndex];

        for (var i = 0; i < shape.Points.Length; i++)
            physicsObject.GetComponent<PhysicsShape>().Points[i].Acceleration -= npc.velocity / 256;
    }
}
