using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.BehaviorTrees;
using Terraria;

namespace SpaceEventMod.Common.Actions;

/// <summary>
/// Makes the npc accelerate towards its target.
/// </summary>
/// <param name="acceleration">Rate of acceleration.</param>
/// <param name="maxSpeed">Maximum speed of the npc.</param>
public class AccelerateTowardsTarget(float acceleration, float maxSpeed) : Node
{
    private float acceleration = acceleration;
    private float maxSpeed = maxSpeed;

    public override NodeState Update(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];

        Vector2 targetCenter = npc.HasNPCTarget ? Main.npc[npc.TranslatedTargetIndex].Center : Main.player[npc.target].Center;

        Vector2 accelerationVector = targetCenter - npc.Center;
        accelerationVector.Normalize();
        accelerationVector *= acceleration;

        Vector2 newVelocity = npc.velocity + accelerationVector;
        float speed = newVelocity.Length();
        newVelocity.Normalize();
        npc.velocity = newVelocity * MathHelper.Clamp(speed, 0, maxSpeed);

        return NodeState.InProgress;
    }
}