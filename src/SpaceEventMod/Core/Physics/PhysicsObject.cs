using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Core.Physics;

internal struct PhysicsObject(PhysicsData[] physicsDatas)
{
    private readonly Dictionary<string, ParameterValue> _localData = new Dictionary<string, ParameterValue>();
    private readonly PhysicsData[] _physicsDatas = physicsDatas;

    public ReadOnlyDictionary<string, ParameterValue> LocalData { get => _localData.AsReadOnly(); }
    public ReadOnlySpan<PhysicsData> PhysicsData { get => _physicsDatas; }

    public int PointCount { get => _physicsDatas.Sum((physicsDataSet) => physicsDataSet.PointCount); }
    public int LinkCount { get => _physicsDatas.Sum((physicsDataSet) => physicsDataSet.LinkCount); }

    public PhysicsObject SetLocalData(string key, ParameterValue value)
    {
        if (_localData.ContainsKey(key))
            _localData[key] = value;
        else
            AddLocalData(key, value);

        return this;
    }

    public PhysicsObject AddLocalData(string key, ParameterValue value)
    {
        _localData.Add(key, value);

        return this;
    }

    public PhysicsObject AddLocalData(params ReadOnlySpan<(string Name, ParameterValue Value)> values)
    {
        foreach (var value in values)
            _localData.Add(value.Name, value.Value);

        return this;
    }

    public Tuple<PhysicsPoint, PhysicsPoint> GetLinkPoints(int dataSet, int linkIndex)
    {
        ILink link = _physicsDatas[dataSet].GetLink(linkIndex);

        if (link is PhysicsLink physicsLink)
            return GetLinkPoints(physicsLink, dataSet);
        else if (link is ControlledPhysicsLink controlledPhysicsLink)
            return GetLinkPoints(controlledPhysicsLink, dataSet);

        return Tuple.Create<PhysicsPoint, PhysicsPoint>(default, default);
    }

    private Tuple<PhysicsPoint, PhysicsPoint> GetLinkPoints(PhysicsLink link, int dataSet)
    {
        int point1Index = link.GetPointIndex(true);
        int point2Index = link.GetPointIndex(false);
        
        return Tuple.Create(_physicsDatas[dataSet].GetPoint(point1Index), _physicsDatas[dataSet].GetPoint(point2Index));
    }

    private Tuple<PhysicsPoint, PhysicsPoint> GetLinkPoints(ControlledPhysicsLink link, int dataSet)
    {
        int pointIndex = link.GetPointIndex(false);
        string controlIndex = link.GetPointIndex(true);

        _localData.TryGetValue(controlIndex, out ParameterValue value);

        return Tuple.Create(new PhysicsPoint(value.Vector2), _physicsDatas[dataSet].GetPoint(pointIndex));
    }
}
