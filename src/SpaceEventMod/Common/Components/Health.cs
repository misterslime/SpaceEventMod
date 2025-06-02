using SpaceEventMod.Core.Props;
using Terraria.Audio;

namespace SpaceEventMod.Common.Components;

/// <summary>
/// Makes this prop have a health bar that destroys the prop on depletion.
/// </summary>
/// <param name="maxHealth">Maximum health of the prop.</param>
/// <param name="deathSound">Sound to be played on prop death. Requires the <see cref="Transformation"/> component to play.</param>
public class Health(int maxHealth, SoundStyle deathSound) : Component
{
    public int Current = maxHealth;
    public int MaxHealth = maxHealth;
    public SoundStyle DeathSound = deathSound;
}

