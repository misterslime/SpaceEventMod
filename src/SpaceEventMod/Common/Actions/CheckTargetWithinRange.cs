using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Check if the target is in range of the npc.
/// </summary>
/// <param name="range">How close the target must be.</param>
public class CheckTargetWithinRange(float range) : Node
{
    private float range = range;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        Vector2 targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        if (npc.Center.WithinRange(targetCenter, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}