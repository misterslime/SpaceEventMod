using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using SpaceEventMod.Core.GameObjects.Stars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Motion;

public struct NearestStarSquidMovement(float jumpDistance, float gravity, int cooldown) : INode
{
    private float jumpDistance = jumpDistance;
    private float gravity = gravity;
    private int cooldown = cooldown;

    public NodeState Update(BehaviorTree parentTree, int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (StarSystem.Stars.Count <= 0 || npc.ModNPC is not IDynamicMotion dynamicMotion || npc.ModNPC is not ITimer timer || npc.ModNPC is not ISquidIdleGravity squidGravity)
            return NodeState.Failure;

        var distanceToStar = float.MaxValue;

        if (timer.Time > 0)
        {
            timer.Time--;

            if (dynamicMotion.TargetPosition.Distance(npc.Center) <= 16)
                dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + new Vector2(0, gravity);

            return NodeState.InProgress;
        }

        Vector2 motionVector = Vector2.Zero;

        foreach (var star in StarSystem.Stars)
        {
            if (Vector2.DistanceSquared(star.GetCenter(), npc.Center) < distanceToStar)
            {
                distanceToStar = Vector2.DistanceSquared(star.GetCenter(), npc.Center);
                Vector2 vectorToStar = star.GetCenter() - npc.Center;
                vectorToStar.Normalize();

                motionVector = vectorToStar * jumpDistance;
            }
        }

        dynamicMotion.TargetPosition = dynamicMotion.TargetPosition + motionVector;
        squidGravity.Gravity = 0;
        timer.Time = cooldown;
        npc.netUpdate = true;

        return NodeState.Success;
    }
}
