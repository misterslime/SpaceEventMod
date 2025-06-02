using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Props;
using Terraria;

namespace SpaceEventMod.Common.Components.Behavior;

/// <summary>
/// Makes this prop something that can be collided with like a platform.<br/>
/// Requires the <see cref="Transformation"/> and <see cref="Hitbox"/> components to function.
/// </summary>
/// <param name="stoodOn">Whether the collider is being stood on.</param>
public class Collider(bool stoodOn) : Component
{
    public bool StoodOn = stoodOn;
}
