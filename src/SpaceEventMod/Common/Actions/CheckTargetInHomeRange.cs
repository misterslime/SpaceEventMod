using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Check if the target is in range of this npc's <see cref="IHasHome.HomePosition"/>.
/// </summary>
/// <param name="range">How close the target must be.</param>
public class CheckTargetInHomeRange(float range) : Node
{
    private float range = range;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IHasHome home)
            return NodeState.Failure;

        Vector2 targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        if (home.HomePosition.WithinRange(targetCenter, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}