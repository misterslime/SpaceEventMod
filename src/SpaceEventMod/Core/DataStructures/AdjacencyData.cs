using System;
using Terraria;

namespace SpaceEventMod.Core.DataStructures;

/// <summary>
/// Retrieves data for every adjacent tile position of the specified coordinates.
/// </summary>
/// <param name="i">Horizontal coordinate.</param>
/// <param name="j">Vertical coordinate.</param>
/// <param name="action">Action that retrieves the desired information from the tiles.</param>
public struct AdjacencyData<T>
{
    public T Top { get; }
    public T Bottom { get; }

    public T Left { get; }
    public T Right { get; }

    public T TopLeft { get; }
    public T TopRight { get; }

    public T BottomLeft { get; }
    public T BottomRight { get; }

    public AdjacencyData(int i, int j, Func<Tile, T> action) 
    {
        if (i == 0 || i == Main.maxTilesX || j == 0 || j == Main.maxTilesY)
            return;

        Top = action(Framing.GetTileSafely(i, j - 1));
        Bottom = action(Framing.GetTileSafely(i, j + 1));

        Left = action(Framing.GetTileSafely(i - 1, j));
        Right = action(Framing.GetTileSafely(i + 1, j));

        TopLeft = action(Framing.GetTileSafely(i - 1, j - 1));
        TopRight = action(Framing.GetTileSafely(i + 1, j - 1));

        BottomLeft = action(Framing.GetTileSafely(i - 1, j + 1));
        BottomRight = action(Framing.GetTileSafely(i + 1, j + 1));
    }
}