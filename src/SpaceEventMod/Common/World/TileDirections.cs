using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.World;

internal static class TileDirections
{
    public static readonly Point[] WithCorners = [
        new Point(0, -1),
        new Point(0, 1),

        new Point(-1, 0),
        new Point(1, 0),

        new Point(-1, -1),
        new Point(1, -1),

        new Point(-1, 1),
        new Point(1, 1)
    ];

    public static readonly Point[] NoCorners = [
        new Point(0, -1),
        new Point(0, 1),

        new Point(-1, 0),
        new Point(1, 0)
    ];
}

