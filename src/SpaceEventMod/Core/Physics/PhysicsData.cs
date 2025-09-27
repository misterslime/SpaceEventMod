using Microsoft.Xna.Framework;
using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpaceEventMod.Core.Physics;

internal struct PhysicsData(PhysicsPoint[] points, ILink[] links)
{
    private readonly PhysicsPoint[] _points = points;
    private readonly ILink[] _links = links;

    public int PointCount { get; init; } = points.Length;
    public int LinkCount { get; init; } = links.Length;

    public ReadOnlySpan<PhysicsPoint> Points { get => _points; }
    public ReadOnlySpan<ILink> Links { get => _links; }

    public PhysicsPoint GetPoint(int index) => _points[index];
    public ILink GetLink(int index) => _links[index];

    public void SetPoint(int index, PhysicsPoint point) => _points[index] = point;
}
