using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.DataStructures;

[StructLayout(LayoutKind.Explicit)]
public struct ParameterValue
{
    [FieldOffset(0)]
    public ParameterValueType Type;

    [FieldOffset(8)]
    public Texture2D Texture2D;

    [FieldOffset(16)]
    public float Float;

    [FieldOffset(16)]
    public int Int;

    [FieldOffset(16)]
    public Vector2 Vector2;

    [FieldOffset(16)]
    public Vector3 Vector3;

    [FieldOffset(16)]
    public Vector4 Vector4;

    [FieldOffset(16)]
    public Matrix Matrix;

    public static implicit operator ParameterValue(float value) => new()
    {
        Type = ParameterValueType.Float,
        Float = value,
    };

    public static implicit operator ParameterValue(int value) => new()
    {
        Type = ParameterValueType.Int,
        Int = value,
    };

    public static implicit operator ParameterValue(Vector2 value) => new()
    {
        Type = ParameterValueType.Vector2,
        Vector2 = value,
    };

    public static implicit operator ParameterValue(Vector3 value) => new()
    {
        Type = ParameterValueType.Vector3,
        Vector3 = value,
    };

    public static implicit operator ParameterValue(Vector4 value) => new()
    {
        Type = ParameterValueType.Vector4,
        Vector4 = value,
    };

    public static implicit operator ParameterValue(Texture2D value) => new()
    {
        Type = ParameterValueType.Texture2D,
        Texture2D = value,
    };

    public static implicit operator ParameterValue(Matrix value) => new()
    {
        Type = ParameterValueType.Matrix,
        Matrix = value,
    };
}

public enum ParameterValueType
{
    Float,
    Int,
    Vector2,
    Vector3,
    Vector4,
    Texture2D,
    Matrix,
}
