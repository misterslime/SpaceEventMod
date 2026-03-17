using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Content.Space.Mechanics.Astralysis;

internal enum MoveState : byte
{
    Floor,
    LeftWall,
    RightWall,
    Ceiling,
    Falling,
    Jumping,
    KickedOut
}
