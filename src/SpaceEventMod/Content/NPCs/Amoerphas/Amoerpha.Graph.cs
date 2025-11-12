using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Content.NPCs.Amoerphas;

internal partial class Amoerpha
{
    [StructLayout(LayoutKind.Explicit, Size = 12)] 
    private struct Edge(float length, int from, int to)
    {
        [FieldOffset(0)] public float Length = length;
        [FieldOffset(4)] public readonly int From = from;
        [FieldOffset(8)] public readonly int To = to;

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To);
        }

        public bool Contains(int i) => From == i || To == i;

        public int Other(int i) => From == i ? To : From;
    }

    private Vector2[] _nodes;
    private List<Edge> _edges;
    private Dictionary<int, List<Edge>> _edgesMap;

    private void Init(int nodes)
    {
        _nodes = new Vector2[nodes];
        _edges = new List<Edge>(nodes);
        _edgesMap = new Dictionary<int, List<Edge>>();
    }

    private List<(int, Edge)> GetLeafNodes()
    {
        List<(int, Edge)> list = new List<(int, Edge)>();

        foreach (int key in _edgesMap.Keys)
        {
            if (_edgesMap[key].Count == 1)
                list.Add((key, _edgesMap[key].First()));
        }

        return list;
    }

    /// <summary>
    /// Returns vector directions of the edges adjacent to this node.
    /// </summary>
    /// <param name="index">Node index to check.</param>
    /// <returns>An array of normalized direction vectors.</returns>
    private Vector2[] GetNodeDirections(int index)
    {
        if (!_edgesMap.ContainsKey(index))
            throw new InvalidOperationException($"Node at index {index} is not a connected node in the graph.");

        List<Edge> edges = _edgesMap[index];
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

    private bool AddEdge(int a, Vector2 b)
    {
        int newNode = -1;

        for (int i = 0; i < _nodes.Length; i++)
        {
            if (!_edgesMap.ContainsKey(i))
            {
                newNode = i;
                _nodes[i] = b;
                break;
            }
        }

        if (newNode == -1)
            return false;

        Vector2 vectorA = _nodes[a];

        float length = (vectorA - b).Length();

        AddEdge(new Edge(length, a, newNode));

        return true;
    }

    private bool AddEdge(Vector2 a, Vector2 b)
    {
        int iterations = 0;

        int[] newNodes = new int[2] { -1, -1 };

        for (int i = 0; i < _nodes.Length; i++)
        {
            if (!_edgesMap.ContainsKey(i))
            {
                newNodes[iterations] = i;
                _nodes[i] = iterations == 0 ? a : b;
                iterations++;
            }

            if (iterations > 1)
            {
                break;
            }
        }

        if (newNodes[0] == -1 || newNodes[1] == -1)
            return false;

        float length = (a - b).Length();

        AddEdge(new Edge(length, newNodes[0], newNodes[1]));

        return true;
    }

    private void AddEdge(Edge edge)
    {
        _edges.Add(edge);
        AddToMap(edge.From, edge);
        AddToMap(edge.To, edge);
    }

    private void RemoveEdge(Edge edge)
    {
        _edges.Remove(edge);
        RemoveFromMap(edge.From, edge);
        RemoveFromMap(edge.To, edge);
    }

    private void ShiftEdgeLength(Edge edge, float newLength)
    {
        if (_edgesMap[edge.To].Count != 1 && _edgesMap[edge.From].Count != 1)
            throw new Exception("Points with more than 1 connection are immutable.");

        Vector2 vectorFrom = _nodes[edge.From];
        Vector2 vectorTo = _nodes[edge.To];

        if (_edgesMap[edge.To].Count == 1)
        {
            var projection = (vectorTo - vectorFrom).SafeNormalize(Vector2.Zero) * newLength;
            _nodes[edge.To] = vectorFrom + projection;
        }
        else if (_edgesMap[edge.From].Count == 1)
        {
            var projection = (vectorFrom - vectorTo).SafeNormalize(Vector2.Zero) * newLength;
            _nodes[edge.From] = vectorTo + projection;
        }

        RemoveEdge(edge);

        edge.Length = newLength;

        AddEdge(edge);
    }

    private void RemoveFromMap(int node, Edge edge)
    {
        List<Edge> edges;
        if (this._edgesMap.TryGetValue(node, out edges))
        {
            edges.Remove(edge);
            this._edgesMap[node] = edges;
        }
    }

    private void AddToMap(int node, Edge edge)
    {
        List<Edge> edges;
        if (this._edgesMap.TryGetValue(node, out edges))
        {
            edges.Add(edge);
            this._edgesMap[node] = edges;
        }
        else
        {
            var el = new List<Edge>();
            el.Add(edge);
            this._edgesMap.Add(node, el);
        }
    }
}
