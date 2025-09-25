using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

internal struct ControlPoint(Vector2 position) : IPoint
{
    public Vector2 Position { get; set; } = position;
}
