using System;
using Terraria;

namespace SpaceEventMod.Core.DataStructures;

/// <summary>
/// Retrieves data for every adjacent tile position of the specified coordinates.
/// </summary>
/// <param name="i">Horizontal coordinate.</param>
/// <param name="j">Vertical coordinate.</param>
/// <param name="action">Action that retrieves the desired information from the tiles.</param>
public struct AdjacencyData<T>(int i, int j, Func<Tile, T> action)
{
    public T Top { get; } = action(Framing.GetTileSafely(i, j - 1));
    public T Bottom { get; } = action(Framing.GetTileSafely(i, j + 1));

    public T Left { get; } = action(Framing.GetTileSafely(i - 1, j));
    public T Right { get; } = action(Framing.GetTileSafely(i + 1, j));

    public T TopLeft { get; } = action(Framing.GetTileSafely(i - 1, j - 1));
    public T TopRight { get; } = action(Framing.GetTileSafely(i + 1, j - 1));

    public T BottomLeft { get; } = action(Framing.GetTileSafely(i - 1, j + 1));
    public T BottomRight { get; } = action(Framing.GetTileSafely(i + 1, j + 1));
}