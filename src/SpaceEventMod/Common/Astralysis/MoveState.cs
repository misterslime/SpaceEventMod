using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Astralysis;

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
