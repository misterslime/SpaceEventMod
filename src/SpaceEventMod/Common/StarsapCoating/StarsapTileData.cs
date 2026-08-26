using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.StarsapCoating;

internal enum StarsapTypes : byte
{
    None = 0,
    Coated = 1
}

[StructLayout(LayoutKind.Explicit)]
internal struct StarsapTileData : ITileData
{
    [FieldOffset(0)]
    private StarsapTypes _data;

    public StarsapTypes Types { get => _data; set => _data = value; }

    public bool Coated
    {
        get => (_data & StarsapTypes.Coated) != 0;
        set => _data = value ? _data | StarsapTypes.Coated : _data & ~StarsapTypes.Coated;
    }
}
