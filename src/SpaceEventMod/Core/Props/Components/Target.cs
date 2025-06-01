namespace SpaceEventMod.Core.Props.Components;

/// <summary>
/// If combined with an <see cref="AlertEvent"/>, it allows you to make sure only specific npcs are affected by the alert.
/// </summary>
/// <param name="npcsToTarget">The int IDs of the npcs to target.</param>
public class Target(params int[] npcsToTarget) : Component
{
    public int[] NPCsToTarget = npcsToTarget;
}
