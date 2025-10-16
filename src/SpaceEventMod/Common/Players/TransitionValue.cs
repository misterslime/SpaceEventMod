using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Players;

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
