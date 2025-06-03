using Terraria.ModLoader;

namespace SpaceEventMod.Core.Behavior.BehaviorTrees;

/// <summary>
/// A behaviour tree that can easily run complex entity behaviour on an npc.
/// </summary>
/// <param name="root">Root node for this tree.</param>
public struct BehaviorTree(INode root)
{
    private INode RootNode = root;

    /// <summary>
    /// Run this every frame, preferably in <see cref="ModNPC.AI()"/> or <see cref="ModNPC.PreAI()"/>.
    /// </summary>
    /// <param name="whoAmI">NPC that this tree will be run on.</param>
    public void Update(int whoAmI)
    {
        if (RootNode != null)
            RootNode.Update(whoAmI);
    }
}