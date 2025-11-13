using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Content.NPCs.Amoerphas;
internal partial class Amoerpha
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    private struct NearestData(int index, Vector2 direction)
    {
        [FieldOffset(0)] public int Index = index;
        [FieldOffset(4)] public Vector2 Direction = direction;
    }

    private const float MAX_EDGE_LENGTH = 64;
    private const float MAX_BODY_LENGTH = 1024;

    public float BodyLength => _edges.Sum(e => e.Length);

    private bool CanGrowNode(int index) => _adjacencyMap[index].Count == 1 && _adjacencyMap[index][0].Length < MAX_EDGE_LENGTH;

    private void GrowNode(NearestData node, Vector2 target, float amount)
    {
        // ensure amount is a positive number bc this method is supposed to *grow* nodes
        amount = MathF.Abs(amount);

        if (CanGrowNode(node.Index))
        {
            Edge edge = _adjacencyMap[node.Index][0];
            ShiftEdgeLength(node.Index, edge, edge.Length + amount);
            return;
        }

        Vector2 nodePosition = _nodes[node.Index];

        Vector2 toTarget = target - nodePosition;
        toTarget = toTarget.SafeNormalize(Vector2.Zero);

        Vector2 newDirection = Vector2.Lerp(toTarget, node.Direction, MathF.Pow(Main.rand.NextFloat(), 2));
        newDirection = newDirection.SafeNormalize(Vector2.Zero);

        AddEdge(node.Index, nodePosition + newDirection * amount);
    }

    /// <summary>
    /// Check if an edge can be shrinked.
    /// Returns: -1 if it cannot be, the index of the movable node if it can.
    /// </summary>
    private int CanShrinkEdge(Edge edge)
    {
        if (_adjacencyMap[edge.From].Count == 1)
            return edge.From;
        else if (_adjacencyMap[edge.To].Count == 1)
            return edge.To;

        return -1;
    }

    private void ShrinkEdges(float amount, int selectedNode)
    {
        List<int> shrinkables = new List<int>();

        foreach (var edge in _edges.ToArray())
        {
            if (edge.Length <= 0)
            {
                RemoveEdge(edge);
                continue;
            }

            int node = CanShrinkEdge(edge);

            if (node != -1 && node != selectedNode)
                shrinkables.Add(node);
        }

        if (shrinkables.Count == 0)
            return;

        amount /= shrinkables.Count;

        foreach (var node in shrinkables)
        {
            Edge edge = _adjacencyMap[node][0];

            ShiftEdgeLength(node, edge, edge.Length - amount);
        }
    }

    private void ShiftEdgeLength(int nodeMoved, Edge edge, float newLength)
    {
        if (_adjacencyMap[edge.To].Count != 1 && _adjacencyMap[edge.From].Count != 1)
            throw new Exception("Points with more than 1 connection are immutable.");

        Vector2 nodePosition = _nodes[nodeMoved];
        Vector2 otherPosition = _nodes[edge.Other(nodeMoved)];

        var projection = (nodePosition - otherPosition).SafeNormalize(Vector2.Zero) * newLength;
        _nodes[nodeMoved] = otherPosition + projection;

        RemoveEdge(edge);
        edge.Length = newLength;
        AddEdge(edge);
    }

    // returns closest angled node if possible
    // returns closest node if not possible
    private NearestData GetNearestNodeToTarget(Vector2 target)
    {
        float smallestHypot = 999999999999f;
        int closestAngle = -1;
        int closestDistance = -1;

        Vector2 direction = Vector2.Zero;

        foreach (Edge edge in _edges)
        {
            CheckEdgeNode(target, edge.From, ref smallestHypot, ref direction, ref closestDistance, ref closestAngle);
            CheckEdgeNode(target, edge.To, ref smallestHypot, ref direction, ref closestDistance, ref closestAngle);
        }

        int node = closestAngle == -1 ? closestDistance : closestAngle;

        return new NearestData(node, direction);
    }

    private void CheckEdgeNode(Vector2 target, int node,
        ref float smallestHypot, ref Vector2 direction, ref int closestDistance, ref int closestAngle)
    {
        Vector2 position = _nodes[node];

        Vector2 toTarget = target - position;

        Vector2 nodeDirection = GetNodeDirection(node, target);

        if (EqualityComparer<Vector2>.Default.Equals(nodeDirection, default(Vector2)))
            return;

        float hypotenuse = nodeDirection.Length();
        nodeDirection = nodeDirection.SafeNormalize(Vector2.Zero);

        bool rightAngle = Vector2.Dot(toTarget.SafeNormalize(Vector2.Zero), nodeDirection) > 0.1f;

        if (smallestHypot > hypotenuse)
        {
            if (rightAngle)
                closestAngle = node;

            smallestHypot = hypotenuse;
            closestDistance = node;
            direction = nodeDirection;
        }
    }

    private Vector2 GetNodeDirection(int node, Vector2 target)
    {
        Vector2 position = _nodes[node];

        Vector2 toTarget = target - position;
        float hypotenuse = toTarget.Length();
        toTarget = toTarget.SafeNormalize(Vector2.Zero);

        Vector2 direction = _adjacencyMap[node].Count switch
        {
            1 => SingleEdgeDirection(node, in position),
            2 => DoubleEdgeDirection(node, in position),
            _ => default
        };

        return direction * hypotenuse;
    }

    private Vector2 SingleEdgeDirection(int node, in Vector2 position)
    {
        Vector2 otherPosition = _nodes[_adjacencyMap[node][0].Other(node)];
        Vector2 direction = (position - otherPosition).SafeNormalize(Vector2.Zero);
        return direction;
    }

    private Vector2 DoubleEdgeDirection(int node, in Vector2 position)
    {
        Vector2[] directions = GetNodeDirections(node);

        if (directions.Length != 2)
            throw new Exception($"WHY NOT 2 WTF!!!!!!!! Was instead {directions.Length}.");

        Vector2 direction = Vector2.Lerp(directions[0], directions[1], 0.5f);
        direction = direction.SafeNormalize(Vector2.Zero);
        direction *= -1;

        return direction;
    }

    /// <summary>
    /// Returns vector directions of the edges adjacent to this node.
    /// </summary>
    /// <param name="index">Node index to check.</param>
    /// <returns>An array of normalized direction vectors.</returns>
    private Vector2[] GetNodeDirections(int index)
    {
        if (!_adjacencyMap.ContainsKey(index))
            throw new InvalidOperationException($"Node at index {index} is not a connected node in the graph.");

        List<Edge> edges = _adjacencyMap[index];
        Vector2[] array = new Vector2[edges.Count];

        for (int i = 0; i < edges.Count; i++)
        {
            int other = edges[i].Other(index);

            Vector2 direction = _nodes[other] - _nodes[index];
            direction = direction.SafeNormalize(Vector2.Zero);

            array[i] = direction;
        }

        return array;
    }
}
