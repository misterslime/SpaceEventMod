using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Leaf.Motion;

/// <summary>
/// Move at constant velocity at the npc's target.
/// </summary>
/// <param name="speed">Speed the npc will move at.</param>
public struct MoveTowardsTarget(float speed) : INode
{
    private float speed = speed;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        npc.velocity = targetCenter - npc.Center;
        npc.velocity.Normalize();
        npc.velocity *= speed;

        return NodeState.InProgress;
    }
}