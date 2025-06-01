using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Target the player if they're close enough to the npc.
/// </summary>
/// <param name="range">Distance to be targeted from.</param>
public class TargetPlayerWithinRange(float range) : Node
{
    private float range = range;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        npc.TargetClosest();

        if (Main.player[npc.target].WithinRange(npc.Center, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}