using Humanizer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using WorldGenSandbox.Utilities;

namespace WorldGenSandbox.SDFs;

internal enum SmoothMinimum : byte
{
    Quadratic, 
    Cubic, 
    Quartic, 
    Circular, 
    Exponential, 
    Sigmoid, 
    SquareRoot, 
    CircularGeometrical,
    Min // literally just dont use a smooth minimum function lma
}

/// <summary>
/// Refers to a set of SDF primitives to be sampled from.
/// </summary>
internal class SdfScene()
{
    private HashSet<ISignedDistance2D> _scene = new HashSet<ISignedDistance2D>();

    /// <summary>
    /// Clear the scene of sdf primitives.
    /// </summary>
    public void Clear() => _scene.Clear();

    /// <summary>
    /// Add an SDF primitive to the scene.
    /// </summary>
    /// <param name="primitive">The SDF primitive shape to add.</param>
    /// <param name="material">Optionally give the primitive a material ID to interpolate with.</param>
    public void AddPrimitive(ISignedDistance2D primitive) => _scene.Add(primitive);

    /// <summary>
    /// Samples the signed distance at the specified position.
    /// </summary>
    public Vector3 Sample(Vector2 position, float smoothness, SmoothMinimum smin)
    {
        Vector3 total = new Vector3(99999, 0, 0);

        foreach (var sdf in _scene)
        {
            var dist = sdf.GetSignedDistance(position);

            total = SMin(total, dist, smoothness);
        }

        return total;
    }

    public static Vector3 SMin(Vector3 a, Vector3 b, float k)
    {
        k *= 4.0f;
        float h = MathF.Max(k - MathF.Abs(a.X - b.X), 0.0f) / (2.0f * k);
        Vector2 g = Vector2.Lerp(new Vector2(a.Y, a.Z), new Vector2(b.Y, b.Z), (a.X < b.X) ? h : 1.0f - h);
        return new Vector3(MathF.Min(a.X, b.X) - h * h * k, g.X, g.Y);
    }
}
