using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Actions.Interfaces;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Motion;

/// <summary>
/// Makes the npc accelerate to a position within range of its <see cref="IHasHome.HomePosition"/>.
/// </summary>
/// <param name="acceleration">Rate of acceleration.</param>
/// <param name="maxSpeed">Maximum speed of the npc.</param>
/// <param name="range">Range that it'll accelerate to.</param>
public struct AccelerateInRangeOfHome(float acceleration, float maxSpeed, float range) : INode
{
    private float acceleration = acceleration;
    private float maxSpeed = maxSpeed;
    private float range = range;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        if (npc.ModNPC is not IHasHome home)
            return NodeState.Failure;

        if (npc.Center.WithinRange(home.HomePosition, range))
            return NodeState.Failure;

        var accelerationVector = home.HomePosition - npc.Center;
        accelerationVector.Normalize();
        accelerationVector *= acceleration;

        var newVelocity = npc.velocity + accelerationVector;
        var speed = newVelocity.Length();
        newVelocity.Normalize();
        npc.velocity = newVelocity * MathHelper.Clamp(speed, 0, maxSpeed);

        return NodeState.InProgress;
    }
}