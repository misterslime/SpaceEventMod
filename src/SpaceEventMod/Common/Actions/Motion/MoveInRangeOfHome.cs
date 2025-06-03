using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Motion;

/// <summary>
/// Move at constant velocity to a certain distance from the npc's <see cref="IHasHome.HomePosition"/>.
/// </summary>
/// <param name="speed">Speed the npc will move at.</param>
/// <param name="range">Target distance from its <see cref="IHasHome.HomePosition"/>.</param>
public struct MoveInRangeOfHome(float speed, float range) : INode
{
    private float speed = speed;
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IHasHome home)
            return NodeState.Failure;

        if (npc.Center.WithinRange(home.HomePosition, range))
            return NodeState.Failure;

        npc.velocity = home.HomePosition - npc.Center;
        npc.velocity.Normalize();
        npc.velocity *= speed;

        return NodeState.InProgress;
    }
}