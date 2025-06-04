using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.GameObjects.Alerts;
using SpaceEventMod.Core.GameObjects.Stars;
using Terraria;
using Terraria.ID;

namespace SpaceEventMod.Common.Actions.Leaf.Targeting;

public struct AggroAnythingMiningStar(float range, params int[] npcTypeExceptions) : INode
{
    public int[] npcTypeExceptions = npcTypeExceptions;
    public float range = range;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (AlertSystem.alerts.Count <= 0)
            return NodeState.Failure;

        var distanceToStar = float.MaxValue;
        int target = -1;

        foreach (var alert in AlertSystem.alerts)
        {
            npc.target = alert.sourceEntity;

            Vector2 targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

            if (Vector2.DistanceSquared(targetCenter, npc.Center) < distanceToStar && Vector2.DistanceSquared(targetCenter, npc.Center) <= range * range)
            {
                distanceToStar = Vector2.DistanceSquared(targetCenter, npc.Center);
                target = alert.sourceEntity;
            }
        }

        npc.target = target;
        npc.targetRect = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].getRect() : Main.player[npc.TranslatedTargetIndex].getRect();

        return NodeState.Success;
    }
}
