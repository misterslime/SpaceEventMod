using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Motion;

/// <summary>
/// Multiply the npc's velocity by some amount.
/// </summary>
/// <param name="amount">The multiplier.</param>
public struct MultiplyVelocity(float amount) : INode
{
    private float amount = amount;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        npc.velocity *= amount;

        return NodeState.InProgress;
    }
}