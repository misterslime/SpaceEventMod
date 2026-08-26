using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Geometry;

internal interface IPoint
{
    public Vector2 Position { get; set; }
}
