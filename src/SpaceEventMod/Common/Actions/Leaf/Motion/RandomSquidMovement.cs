using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Content.NPCs;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Motion;

public struct RandomSquidMovement(float jumpDistance, float gravity, int cooldown) : INode
{
    private float jumpDistance = jumpDistance;
    private float gravity = gravity;
    private int cooldown = cooldown;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IDynamicMotion dynamicMotion || npc.ModNPC is not ITimer timer || npc.ModNPC is not ISquidIdleGravity squidGravity || npc.ModNPC is not Manaphage manaphage)
            return NodeState.Failure;

        if (timer.Time > 0)
        {
            timer.Time--;

            if (dynamicMotion.TargetPosition.Distance(npc.Center) <= 16)
                dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + new Vector2(0, gravity);

            return NodeState.InProgress;
        }

        dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + Main.rand.NextVector2Unit() * jumpDistance;
        squidGravity.Gravity = 0;
        timer.Time = cooldown;
        npc.netUpdate = true;
        return NodeState.Success;
    }
}
