namespace SpaceEventMod.Core.Behavior.BehaviorTrees;

public class BehaviorTree
{
    private Node RootNode;

    public BehaviorTree(Node root)
    {
        RootNode = root;
    }

    public void Update(int whoAmI)
    {
        if (RootNode != null)
            RootNode.Update(whoAmI);
    }
}
