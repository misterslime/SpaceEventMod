using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Multiply the npc's velocity by some amount.
/// </summary>
/// <param name="amount">The multiplier.</param>
public class MultiplyVelocity(float amount) : Node
{
    private float amount = amount;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        npc.velocity *= amount;

        return NodeState.InProgress;
    }
}