using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

public class CheckTargetInHomeRange : Node
{
    private float range;

    public CheckTargetInHomeRange(float range)
    {
        this.range = range;
    }

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
