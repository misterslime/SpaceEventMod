using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SpaceEventMod.Core.SourceGeneration;

/// <summary>
/// Provides metadata info for hook sourcegen per attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DetourInfoAttribute : Attribute
{
    /// <summary>
    /// Delegate type to compare a method signature to.
    /// </summary>
    public Type DelegateType { get; set; }

    /// <summary>
    /// Name of the event thats being subscribed to.
    /// Includes namespace and type containing the event.
    /// </summary>
    public string FullHookName { get; set; }
}

/// <summary>
/// Inherited by all detouring attributes. Provides information for handling
/// the targeted method and event subscription.
/// </summary>
[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public abstract class BaseDetourAttribute : Attribute
{
    /// <summary>
    /// Which side this should be loaded on.
    /// </summary>
    public bool Side { get; set; }

    public DetourInfoAttribute? DetourInfo { get; set; }

    public BaseDetourAttribute()
    {
        Side = true;
        DetourInfo = GetType().GetCustomAttribute<DetourInfoAttribute>(inherit: false);
    }
}
