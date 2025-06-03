using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Targeting;

/// <summary>
/// Target the player if they're close enough to the npc.
/// </summary>
/// <param name="range">Distance to be targeted from.</param>
public struct TargetPlayerWithinRange(float range) : INode
{
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        npc.TargetClosest();

        if (Main.player[npc.target].WithinRange(npc.Center, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}