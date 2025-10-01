using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics.Interfaces;

internal interface IPass
{
    public int Steps { get; init; }

    public void Pass(PhysicsObject physicsObject);
}
