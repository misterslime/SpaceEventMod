using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SpaceEventMod.Core.Props.Components;

public enum AlertType
{
    Annoyance = 0,
    PotentialDanger = 1,
    Danger = 2
}

public class AlertEvent : Component
{
    public AlertType Type;
    public float Range;
    public int Lifespan;
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

    public static List<AlertEvent> GetAlertEventsInRange(Vector2 position, float rangeExtension = 0f)
    {
        List<AlertEvent> alertEvents = new List<AlertEvent>();

        foreach (var component in ComponentManager.GetComponents<AlertEvent>())
        {
            if (MathF.Abs((component.GetComponent<Transformation>().Position - position).LengthSquared()) <= Math.Pow(component.Range + rangeExtension, 2))
                alertEvents.Add(component);
        }

        return alertEvents;
    }
}
