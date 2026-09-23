using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WorldGenSandbox.Managers;
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

internal partial class World
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
        //SlimeMoldGen.Generate();
        Generated = true;
    }

    public void DrawWorld(object? sender, DrawEventArgs e)
    {
        e.SpriteBatch.Draw(e.Pixel, new Rectangle(0, 0, Globals.World.MaxTilesX, Globals.World.MaxTilesY), Color.DarkBlue * 0.5f);

        Dictionary<TileTypes, Color> colors = new Dictionary<TileTypes, Color>();

        Color wallColor = Color.Gray * 0.6f;
        wallColor.A = 255;

        colors.Add(TileTypes.Empty, Color.Black);
        colors.Add(TileTypes.Cosmostone, Color.Gray);
        colors.Add(TileTypes.CosmostoneWall, wallColor);
        colors.Add(TileTypes.Cosmoss, Color.LightCoral);
        colors.Add(TileTypes.HerbCell, Color.Turquoise);
        colors.Add(TileTypes.Stone, Color.White);
        colors.Add(TileTypes.Mud, new Color(92, 68, 73));
        colors.Add(TileTypes.SlimeMold, Color.Yellow);

        for (int i = 0; i < Globals.World.MaxTilesX; ++i)
        {
            for (int j = 0; j < Globals.World.MaxTilesY; ++j)
            {
                if (Globals.World.Tiles[i, j] == TileTypes.Empty)
                    continue;

                e.SpriteBatch.Draw(e.Pixel, new Vector2(i, j), colors[Globals.World.Tiles[i, j]]);
            }
        }

        e.SpriteBatch.Draw(e.Pixel, new Rectangle(0, 0, Globals.World.MaxTilesX, 40), Color.White * 0.25f);
        e.SpriteBatch.Draw(e.Pixel, new Rectangle(0, 0, 40, Globals.World.MaxTilesY), Color.White * 0.25f);
        e.SpriteBatch.Draw(e.Pixel, new Rectangle(0, Globals.World.MaxTilesY - 40, Globals.World.MaxTilesX, 40), Color.White * 0.25f);
        e.SpriteBatch.Draw(e.Pixel, new Rectangle(Globals.World.MaxTilesX - 40, 0, 40, Globals.World.MaxTilesY), Color.White * 0.25f);
    }
}
