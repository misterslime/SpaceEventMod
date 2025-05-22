using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

public class CheckTargetWithinRange : Node
{
    private float range;

    public CheckTargetWithinRange(float range)
    {
        this.range = range;
    }

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        Vector2 targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        if (npc.Center.WithinRange(targetCenter, range))
            return NodeState.Success;

        return NodeState.Failure;
    }
}
