using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

public class MoveInRangeOfHome : Node
{
    private float speed;
    private float range;

    public MoveInRangeOfHome(float speed, float range)
    {
        this.speed = speed;
        this.range = range;
    }

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IHasHome home)
            return NodeState.Failure;

        if (npc.Center.WithinRange(home.HomePosition, range))
            return NodeState.Failure;

        npc.velocity = home.HomePosition - npc.Center;
        npc.velocity.Normalize();
        npc.velocity *= speed;

        return NodeState.InProgress;
    }
}
