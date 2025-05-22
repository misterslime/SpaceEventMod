using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

public class AccelerateInRangeOfHome : Node
{
    private float acceleration;
    private float maxSpeed;
    private float range;

    public AccelerateInRangeOfHome(float acceleration, float maxSpeed, float range)
    {
        this.acceleration = acceleration;
        this.maxSpeed = maxSpeed;
        this.range = range;
    }

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IHasHome home)
            return NodeState.Failure;

        if (npc.Center.WithinRange(home.HomePosition, range))
            return NodeState.Failure;

        Vector2 accelerationVector = home.HomePosition - npc.Center;
        accelerationVector.Normalize();
        accelerationVector *= acceleration;

        Vector2 newVelocity = npc.velocity + accelerationVector;
        float speed = newVelocity.Length();
        newVelocity.Normalize();
        npc.velocity = newVelocity * MathHelper.Clamp(speed, 0, maxSpeed);

        return NodeState.InProgress;
    }
}