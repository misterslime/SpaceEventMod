using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions.Motion;

/// <summary>
/// Makes the npc accelerate towards its target.
/// </summary>
/// <param name="acceleration">Rate of acceleration.</param>
/// <param name="maxSpeed">Maximum speed of the npc.</param>
public struct AccelerateTowardsTarget(float acceleration, float maxSpeed) : INode
{
    private float acceleration = acceleration;
    private float maxSpeed = maxSpeed;

    public NodeState Update(int whoAmI)
    {
        var npc = Main.npc[whoAmI];

        var targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        var accelerationVector = targetCenter - npc.Center;
        accelerationVector.Normalize();
        accelerationVector *= acceleration;

        var newVelocity = npc.velocity + accelerationVector;
        var speed = newVelocity.Length();
        newVelocity.Normalize();
        npc.velocity = newVelocity * MathHelper.Clamp(speed, 0, maxSpeed);

        return NodeState.InProgress;
    }
}