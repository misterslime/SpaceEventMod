using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Motion;

public struct TargetedSquidMovement(float jumpDistance, float gravity, int cooldown, float targetDistance, bool towards = true) : INode
{
    private float jumpDistance = jumpDistance;
    private float gravity = gravity;
    private int cooldown = cooldown;
    private float targetDistance = targetDistance;
    private bool towards = towards;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IDynamicMotion dynamicMotion || npc.ModNPC is not ITimer timer || !npc.HasValidTarget)
            return NodeState.Failure;

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.TranslatedTargetIndex].Center;

        if (targetCenter.WithinRange(npc.Center, targetDistance) && towards)
            return NodeState.Success;
        else if (!targetCenter.WithinRange(npc.Center, targetDistance) && !towards)
            return NodeState.Success;

        if (timer.Time > 0)
        {
            timer.Time--;

            if (dynamicMotion.TargetPosition.Distance(npc.Center) <= 16)
                dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + new Vector2(0, gravity);

            return NodeState.InProgress;
        }

        var vectorToTarget = targetCenter - npc.Center;
        vectorToTarget.Normalize();

        dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + (towards ? vectorToTarget * jumpDistance : -vectorToTarget * jumpDistance);
        timer.Time = cooldown;
        npc.netUpdate = true;

        return NodeState.InProgress;
    }
}

