using SpaceEventMod.Common.Mechanics.StarsapCoating;
using System.Runtime.InteropServices;

namespace SpaceEventMod.Common.Mechanics.Astralysis;

[StructLayout(LayoutKind.Explicit)]
internal struct TransitionValue
{
    [FieldOffset(0)]
    public TransitionValueType Type;

    [FieldOffset(1)]
    public StarsapTile StarsapTile;

    [FieldOffset(2)]
    public bool Bool;

    public static implicit operator TransitionValue(bool value) => new()
    {
        Type = TransitionValueType.Bool,
        Bool = value,
    };

    public static implicit operator TransitionValue(StarsapTile value) => new()
    {
        Type = TransitionValueType.StarsapTile,
        StarsapTile = value,
    };
}

internal enum TransitionValueType : byte
{
    Bool,
    StarsapTile
}
