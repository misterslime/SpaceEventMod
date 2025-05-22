using System;
using Terraria;

namespace SpaceEventMod.Core.DataStructures;

public ref struct AdjacencyData<T>
{
    public T Top;
    public T Bottom;
    public T Left;
    public T Right;
    public T TopLeft;
    public T TopRight;
    public T BottomLeft;
    public T BottomRight;

    public AdjacencyData(int i, int j, Func<Tile, T> action)
    {
        this.Top = action(Framing.GetTileSafely(i, j - 1));
        this.Bottom = action(Framing.GetTileSafely(i, j + 1));

        this.Left = action(Framing.GetTileSafely(i - 1, j));
        this.Right = action(Framing.GetTileSafely(i + 1, j));

        this.TopLeft = action(Framing.GetTileSafely(i - 1, j - 1));
        this.TopRight = action(Framing.GetTileSafely(i + 1, j - 1));

        this.BottomLeft = action(Framing.GetTileSafely(i - 1, j - 1));
        this.BottomRight = action(Framing.GetTileSafely(i + 1, j - 1));
    }
}
