using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props;
using System;
using System.Collections.Generic;

namespace SpaceEventMod.Common.Components.Events;

/// <summary>
/// Makes this prop an event that certain creatures have responses to.
/// </summary>
/// <param name="type">The type of response this event will cause.</param>
/// <param name="range">Range of detection for this event.</param>
/// <param name="lifespan">How long this event will last.</param>
public class AlertEvent(AlertEventType type, float range, int lifespan) : Component
{
    public AlertEventType Type = type;
    public float Range = range;
    public int Lifespan = lifespan;
}
