using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Move at constant velocity at the npc's target.
/// </summary>
/// <param name="speed">Speed the npc will move at.</param>
public class MoveTowardsTarget(float speed) : Node
{
    private float speed = speed;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        Vector2 targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        npc.velocity = targetCenter - npc.Center;
        npc.velocity.Normalize();
        npc.velocity *= speed;

        return NodeState.InProgress;
    }
}