using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

internal sealed class PhysicsData()
{
    private readonly Dictionary<string, ParameterValue> _localData = new Dictionary<string, ParameterValue>();
    private readonly List<PhysicsPoint> _points = new List<PhysicsPoint>();
    private readonly List<ILink> _links = new List<ILink>();

    public PhysicsPoint GetPoint(string key) => new PhysicsPoint(_localData[key].Vector2, true);
    public PhysicsPoint GetPoint(int index) => _points[index];

    public void SetPoint(string key, in Vector2 point) => _localData[key] = point;
    public void SetPoint(int index, in PhysicsPoint point) => _points[index] = point;

    public int PointCount { get => _points.Count; }
    public int LinkCount { get => _links.Count; }
    public ReadOnlyCollection<PhysicsPoint> Points { get => _points.AsReadOnly(); }
    public ReadOnlyCollection<ILink> Links { get => _links.AsReadOnly(); }
    public ReadOnlyDictionary<string, ParameterValue> LocalData { get => _localData.AsReadOnly(); }

    public PhysicsData AddLocalData(string key, ParameterValue value)
    {
        _localData.Add(key, value);

        return this;
    }

    public PhysicsData AddLocalData(params ReadOnlySpan<(string Name, ParameterValue Value)> values)
    {
        foreach (var value in values)
            _localData.Add(value.Name, value.Value);

        return this;
    }

    public PhysicsData AddPoint(PhysicsPoint point)
    {
        _points.Add(point);
        return this;
    }

    public PhysicsData AddLink<T, U>(T point1, U point2, float length)
    {
        _links.Add(new PhysicsLink<T, U>(point1, point2, length));

        return this;
    }
}
