using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Geometry;

internal interface ITriangulate
{
    public ReadOnlySpan<Vector2> Points { get; }

    public bool PointInside(Vector2 point);
}
