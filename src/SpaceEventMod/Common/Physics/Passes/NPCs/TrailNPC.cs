using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Attributes;
using SpaceEventMod.Common.Physics.Components;
using SpaceEventMod.Common.Physics.Interfaces;
using Terraria;

namespace SpaceEventMod.Common.Physics.Passes.NPCs;

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
