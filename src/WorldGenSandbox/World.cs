using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WorldGenSandbox.Utilities;
using WorldGenSandbox.WorldGen;

namespace WorldGenSandbox;

public enum TileTypes : byte
{
    Empty,
    Cosmostone,
    CosmostoneWall,
    Cosmoss,
    HerbCell,
    Opening,
    Mud,
    Stone,
    SlimeMold
}

public partial class World
{
    private TileTypes[,] _tiles;

    public int MaxTilesX { get; private set; }
    public int MaxTilesY { get; private set; }
    public bool Generated { get; private set; }
    public TileTypes[,] Tiles { get => _tiles; set => _tiles = value; }

    public World(int width, int height)
    {
        _tiles = new TileTypes[width, height];
        Generated = false;

        MaxTilesX = width;
        MaxTilesY = height;
    }

    public void TryGenerate()
    {
        if (Generated)
            return;

        CellularGrowthGen.Generate();
        SlimeMoldGen.Generate();
        Generated = true;
    }
}
