using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Linq;
using Terraria;
using Terraria.Utilities;

namespace SpaceEventMod.Common.Actions.Leaf.Targeting;

/// <summary>
/// Targets certain npcs within a certain distance of the npc.
/// </summary>
/// <param name="range">Distance to be targeted at.</param>
/// <param name="npcsToTarget">NPC types to target.</param>
public struct TargetNPCWithinRange(float range, params int[] npcsToTarget) : INode
{
    private float range = range;
    private int[] npcsToTarget = npcsToTarget;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        var leaf = this;

        bool npcSearchFilter(NPC nPC)
        {
            if (leaf.npcsToTarget.Length == 0)
                return nPC.WithinRange(npc.Center, leaf.range);
            else
                return leaf.npcsToTarget.Contains(nPC.type) && nPC.WithinRange(npc.Center, leaf.range);
        }

        var results = NPCUtils.SearchForTarget(npc, NPCUtils.TargetSearchFlag.NPCs, npcFilter: npcSearchFilter);
        if (results.FoundTarget)
        {
            var targetType = results.NearestTargetType;

            npc.target = results.NearestTargetIndex;
            npc.targetRect = results.NearestTargetHitbox;

            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}