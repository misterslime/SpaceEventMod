using SpaceEventMod.Core.Behavior.BehaviorTrees;
using System;
using System.Linq;
using Terraria;
using static Terraria.Utilities.NPCUtils;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Targets certain npcs within a certain distance of the npc.
/// </summary>
/// <param name="range">Distance to be targeted at.</param>
/// <param name="npcsToTarget">NPC types to target.</param>
public class TargetNPCWithinRange(float range, params int[] npcsToTarget) : Node
{
    private float range = range;
    private int[] npcsToTarget = npcsToTarget;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        bool npcSearchFilter(NPC nPC)
        {
            if (npcsToTarget.Length == 0)
                return nPC.WithinRange(npc.Center, range);
            else
                return npcsToTarget.Contains(nPC.type) && nPC.WithinRange(npc.Center, range);
        }

        TargetSearchResults results = SearchForTarget(npc, TargetSearchFlag.NPCs, npcFilter: npcSearchFilter);
        if (results.FoundTarget)
        {
            TargetType targetType = results.NearestTargetType;

            npc.target = results.NearestTargetIndex;
            npc.targetRect = results.NearestTargetHitbox;

            return NodeState.Success;
        }

        return NodeState.Failure;
    }
}