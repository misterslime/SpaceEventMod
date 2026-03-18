using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Content.CellularGrowth.World;
public enum AsteroidCaveType : byte
{
    None,
    Porous,
    Hollow
}

public struct AsteroidGenValues
{
    public Vector2 position;
    public Point tilePosition;
    public Point start;
    public Point end;
    public int width;
    public int height;
    public float noiseSample;
    public AsteroidCaveType caveType;

    public AsteroidGenValues(Vector2 position, FastNoiseLite noise, int maxTilesX, int maxTilesY)
    {
        int halfWidth = (int)(WorldGen.genRand.Next(16, 25));
        int halfHeight = (int)(WorldGen.genRand.Next(16, 25));

        this.position = position;
        this.tilePosition = new Point((int)(position.X), (int)(position.Y));

        int startX = Math.Max(0, tilePosition.X - halfWidth);
        int startY = Math.Max(0, tilePosition.Y - halfHeight);

        int endX = Math.Min(maxTilesX - 1, tilePosition.X + halfWidth);
        int endY = Math.Min(maxTilesY - 1, tilePosition.Y + halfHeight);

        this.width = endX - startX;
        this.height = endY - startY;
        this.start = new Point(startX, startY);
        this.end = new Point(endX, endY);

        AsteroidCaveType type = AsteroidCaveType.Hollow;

        if (WorldGen.genRand.Next(2) == 0)
            type = AsteroidCaveType.Porous;

        /*if (WorldGen.genRand.Next(8) == 0)
            type = AsteroidCaveType.None;*/

        this.caveType = type;
        this.noiseSample = noise.GetNoise(position.X, position.Y) * 0.5f + 0.5f;
    }
}

