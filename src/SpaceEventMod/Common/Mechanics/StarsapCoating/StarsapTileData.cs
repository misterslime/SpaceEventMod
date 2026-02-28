using System.Runtime.InteropServices;
using Terraria;

namespace SpaceEventMod.Common.Mechanics.StarsapCoating;

internal enum StarsapTypes : byte
{
    None = 0,
    Coated = 1
}

[StructLayout(LayoutKind.Explicit)]
internal struct StarsapTileData : ITileData
{
    [field: FieldOffset(0)]
    public StarsapTypes Types { get; set; }

    public bool Coated
    {
        get => (Types & StarsapTypes.Coated) != 0;
        set => Types = value ? Types | StarsapTypes.Coated : Types & ~StarsapTypes.Coated;
    }
}
