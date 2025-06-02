using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;

namespace SpaceEventMod.Common.Components;

public enum AlertType
{
    Annoyance = 0,
    PotentialDanger = 1,
    Danger = 2
}

/// <summary>
/// Makes this prop an event that certain creatures have responses to.
/// </summary>
/// <param name="type">The type of response this event will cause.</param>
/// <param name="range">Range of detection for this event.</param>
/// <param name="lifespan">How long this event will last.</param>
public class AlertEvent(AlertType type, float range, int lifespan) : Component
{
    public AlertType Type = type;
    public float Range = range;
    public int Lifespan = lifespan;
}

public class AlertEventSystem : ComponentSystem<AlertEvent>
{
    public override void PostUpdateNPCs()
    {
        foreach (var component in components)
        {
            if (component.Lifespan > 0)
            {
                component.Lifespan -= 1;
                continue;
            }

            ComponentManager.QueuePropRemoval(component.prop);
        }
    }

    /// <summary>
	/// Gets a list of <see cref="AlertEvent"/>s that are within range of <paramref name="position"/>.
	/// </summary>
    /// <param name="position">Position to check from.</param>
	/// <param name="rangeExtension">Use this to make alert events be detected further. Defaults to 0.</param>
    /// <returns>A list of <see cref="AlertEvent"/>s, or an empty list if none are in range.</returns>
    public static List<AlertEvent> GetAlertEventsInRange(Vector2 position, float rangeExtension = 0f)
    {
        var alertEvents = new List<AlertEvent>();

        foreach (var component in ComponentManager.GetComponents<AlertEvent>())
        {
            if (MathF.Abs((component.GetComponent<Transformation>().Position - position).LengthSquared()) <= Math.Pow(component.Range + rangeExtension, 2))
                alertEvents.Add(component);
        }

        return alertEvents;
    }
}
