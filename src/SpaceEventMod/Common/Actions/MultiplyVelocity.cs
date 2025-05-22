using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

public class MultiplyVelocity : Node
{
    private float amount;

    public MultiplyVelocity(float amount)
    {
        this.amount = amount;
    }

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        npc.velocity *= amount;

        return NodeState.InProgress;
    }
}