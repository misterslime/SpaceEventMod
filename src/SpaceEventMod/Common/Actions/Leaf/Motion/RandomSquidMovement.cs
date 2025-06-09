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

        if (npc.ModNPC is not IDynamicMotion dynamicMotion || npc.ModNPC is not ITimer timer || npc.ModNPC is not Manaphage manaphage)
            return NodeState.Failure;

        if (timer.Time > 0)
        {
            timer.Time--;

            if (dynamicMotion.TargetPosition.Distance(npc.Center) <= 16)
                dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + new Vector2(0, gravity);

            if (npc.ModNPC is IDynamicStretch squidAnimationp)
            {
                if (timer.Time < 15)
                    squidAnimationp.TargetStretching = new Vector2(1.1f, 0.75f);
                else if (timer.Time >= cooldown - 5)
                    squidAnimationp.TargetStretching = new Vector2(0.8f, 1.25f);
                else
                    squidAnimationp.TargetStretching = Vector2.One;
            }

            return NodeState.InProgress;
        }

        dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + Main.rand.NextVector2Unit() * jumpDistance;
        timer.Time = cooldown;
        npc.netUpdate = true;

        return NodeState.Success;
    }
}
