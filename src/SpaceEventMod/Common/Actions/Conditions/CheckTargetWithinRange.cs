using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Conditions;

/// <summary>
/// Check if the target is in range of the npc.
/// </summary>
/// <param name="range">How close the target must be.</param>
public struct CheckTargetWithinRange(float range) : INode
{
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        if (npc.Center.WithinRange(targetCenter, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}