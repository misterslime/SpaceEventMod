using SpaceEventMod.Common.Components.Animation;
using SpaceEventMod.Core.Props;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace SpaceEventMod.Common.Components.Behavior;

/// <summary>
/// Makes this prop able to be mined with a pickaxe.<br/>
/// Requires the <see cref="Transformation"/>, <see cref="Health"/>, and <see cref="Hitbox"/> components to function.
/// If the prop has the <see cref="DirectionalShake"/> component then mining will cause shaking.
/// </summary>
public class Mineable() : Component
{
}
