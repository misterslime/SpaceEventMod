using SpaceEventMod.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Players;

internal enum MoveState : byte
{
    Floor,
    LeftWall,
    RightWall,
    Ceiling,
    Falling,
    Jumping
}
