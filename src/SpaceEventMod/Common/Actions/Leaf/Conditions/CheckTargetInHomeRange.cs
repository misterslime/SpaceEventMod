using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Conditions;

/// <summary>
/// Check if the target is in range of this npc's <see cref="IHasHome.HomePosition"/>.
/// </summary>
/// <param name="range">How close the target must be.</param>
public struct CheckTargetInHomeRange(float range) : INode
{
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IHasHome home)
            return NodeState.Failure;

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        if (home.HomePosition.WithinRange(targetCenter, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}