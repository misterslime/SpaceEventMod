using System.Collections.Generic;

namespace SpaceEventMod.Core.Behavior.BehaviorTrees;

public abstract class Node
{
    private NodeState State { get; set; }
    public Node Parent;

    private List<Node> Children = new List<Node>();
    private Dictionary<string, object> Context = new Dictionary<string, object>();

    public virtual NodeState Update(int whoAmI) => NodeState.Failure;

    public Node()
    {

    }

    public Node(List<Node> children)
    {
        foreach (Node childNode in children)
        {
            childNode.Parent = this;
            Children.Add(childNode);
        }
    }

    /// <summary>
    /// Removes an entry from the node's dictionary.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>If the removal was successful, usually if the key was found.</returns>
    public bool ClearData(string key)
    {
        if (Context.ContainsKey(key))
        {
            Context.Remove(key);
            return true;
        }
        else if (Parent != null)
            return Parent.ClearData(key);

        return false;
    }

    /// <summary>
    /// Gets data from either this or one of the parent's dictionaries.
    /// </summary>
    /// <param name="key">The key to get.</param>
    /// <returns>The data stored.</returns>
    public object GetData(string key)
    {
        if (Context.TryGetValue(key, out object val))
            return val;
        else if (Parent != null)
            return Parent.GetData(key);

        return null;
    }

    public void SetData(string key, object value)
    {
        Context[key] = value;
    }
}
