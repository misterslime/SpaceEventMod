using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Makes the npc accelerate to a position within range of its <see cref="IHasHome.HomePosition"/>.
/// </summary>
/// <param name="acceleration">Rate of acceleration.</param>
/// <param name="maxSpeed">Maximum speed of the npc.</param>
/// <param name="range">Range that it'll accelerate to.</param>
public class AccelerateInRangeOfHome(float acceleration, float maxSpeed, float range) : Node
{
    private float acceleration = acceleration;
    private float maxSpeed = maxSpeed;
    private float range = range;

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