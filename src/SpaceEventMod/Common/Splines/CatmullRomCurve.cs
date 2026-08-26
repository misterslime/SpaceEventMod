using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SpaceEventMod.Common.Splines;

internal ref struct CatmullRomCurve
{
    private readonly ReadOnlySpan<Vector2> _controlPoints;
    private bool _looping;

    public readonly Vector2 this[int x]
    {
        get => _controlPoints[x];
    }

    public int ControlPointCount => _controlPoints.Length;

    public CatmullRomCurve(ReadOnlySpan<Vector2> controls, bool looping)
    {
        if (controls.Length < 4)
        {
            throw new ArgumentException("catmull roms require at least four control points.", nameof(controls));
        }

        _controlPoints = controls;
        _looping = looping;
    }

    public List<Vector2> GetPoints(int division)
    {
        var points = new List<Vector2>();

        for (var i = 0; i < _controlPoints.Length; i++)
        {
            if (!_looping && (i == 0 || i == _controlPoints.Length - 2 || i == _controlPoints.Length - 1))
                continue;

            var list = new List<Vector2>();

            var p0 = _controlPoints[(i - 1 + _controlPoints.Length) % _controlPoints.Length];
            var p1 = _controlPoints[i];
            var p2 = _controlPoints[(i + 1) % _controlPoints.Length];
            var p3 = _controlPoints[(i + 2) % _controlPoints.Length];

            var resolution = 1f / division;

            for (var j = 1; j < division; j++)
            {
                var t = j * resolution;

                list.Add(Interpolate(p0, p1, p2, p3, t));
            }

            points.AddRange(list);
        }

        return points;
    }

    private static Vector2 Interpolate(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        //The coefficients of the cubic polynomial
        var a = 2f * p1;
        var b = p2 - p0;
        var c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        var d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        var pos = 0.5f * (a + b * t + c * t * t + d * t * t * t);

        return pos;
    }
}
