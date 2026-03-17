using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Content.CellularGrowth.NPCs.Amoerphas;

internal partial class Amoerpha
{
    private struct Entry(int node, float centrality) : IComparable<Entry>
    {
        public readonly int Node = node;
        public readonly float Centrality = centrality;

        public int CompareTo(Entry other) => Centrality.CompareTo(other.Centrality);
    }

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

    private static int s_lastId = 0;

    private List<Edge> _edges;
    private Dictionary<int, Vector2> _nodes;
    private Dictionary<int, List<Edge>> _adjacencyMap;

    private static int GenerateId() => Interlocked.Increment(ref s_lastId);

    private void Init()
    {
        _edges = new List<Edge>();
        _nodes = new Dictionary<int, Vector2>();
        _adjacencyMap = new Dictionary<int, List<Edge>>();
    }

    private Edge AddEdge(int a, Vector2 b)
    {
        int newNode = GenerateId();

        _nodes.Add(newNode, b);

        Vector2 vectorA = _nodes[a];

        float length = (vectorA - b).Length();

        Edge edge = new Edge(length, a, newNode);

        AddEdge(edge);

        return edge;
    }

    private Edge AddEdge(Vector2 a, Vector2 b)
    {
        int nodeA = GenerateId();
        int nodeB = GenerateId();

        _nodes.Add(nodeA, a);
        _nodes.Add(nodeB, b);

        float length = (a - b).Length();

        Edge edge = new Edge(length, nodeA, nodeB);

        AddEdge(edge);

        return edge;
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

    private void RemoveFromMap(int node, Edge edge)
    {
        List<Edge> edges;
        if (this._adjacencyMap.TryGetValue(node, out edges))
        {
            edges.Remove(edge);
            this._adjacencyMap[node] = edges;
        }
    }

    private void AddToMap(int node, Edge edge)
    {
        List<Edge> edges;
        if (this._adjacencyMap.TryGetValue(node, out edges))
        {
            edges.Add(edge);
            this._adjacencyMap[node] = edges;
        }
        else
        {
            var el = new List<Edge>();
            el.Add(edge);
            this._adjacencyMap.Add(node, el);
        }
    }

    private float GetDegreeCentrality(int node)
    {
        if (this._nodes.Count == 0)
            return 0;

        var adjNodes = GetAdjacentNodes(node);
        return adjNodes.Count * 1.0f / this._nodes.Count;
    }

    private List<Entry> GetDegreeCentrality()
    {
        var l = new List<float>();
        if (this._nodes.Count == 0)
        {
            return new List<Entry>();
        }

        return this._nodes
            .Select(node => new Entry(node.Key, this.GetDegreeCentrality(node.Key)))
            .ToList();
    }

    private List<int> GetAdjacentNodes(int node, bool isDirected = false)
    {
        var adjacent = new HashSet<int>();
        List<Edge> edges;
        if (this._adjacencyMap.TryGetValue(node, out edges))
        {
            foreach (var e in edges)
                adjacent.UnionWith([e.To, e.From]);

            adjacent.Remove(node);
        }

        return new List<int>(adjacent);
    }
}
