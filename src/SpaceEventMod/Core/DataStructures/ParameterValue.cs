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

    public static explicit operator float(ParameterValue value) => value.Float;
    public static explicit operator int(ParameterValue value) => value.Int;
    public static explicit operator Vector2(ParameterValue value) => value.Vector2;
    public static explicit operator Vector3(ParameterValue value) => value.Vector3;
    public static explicit operator Vector4(ParameterValue value) => value.Vector4;
    public static explicit operator Texture2D(ParameterValue value) => value.Texture2D;
    public static explicit operator Matrix(ParameterValue value) => value.Matrix;

    public override readonly string ToString() => Type switch
    {
        ParameterValueType.Float => $"{Float}",
        ParameterValueType.Int => $"{Int}",
        ParameterValueType.Vector2 => $"{Vector2}",
        ParameterValueType.Vector3 => $"{Vector3}",
        ParameterValueType.Vector4 => $"{Vector4}",
        ParameterValueType.Texture2D => $"{Texture2D}",
        ParameterValueType.Matrix => $"{Matrix}",
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
