using SpaceEventMod.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IDynamicMotion
{
    public Vector2Dynamics SecondOrderSolver { get; set; }
}
