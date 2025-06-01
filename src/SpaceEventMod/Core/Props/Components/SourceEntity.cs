namespace SpaceEventMod.Core.Props.Components;

/// <summary>
/// Makes the prop have an npc or player as its source and stores their whoAmI.
/// </summary>
/// <param name="whoAmI">The target ID of the source.</param>
public class SourceEntity(int whoAmI) : Component
{
    public int WhoAmI;
}
