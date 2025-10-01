using SpaceEventMod.Core.Physics;
using SpaceEventMod.Core.Physics.Passes.Integrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Animation.SecondOrderDynamics;

internal class SecondOrderDynamics : ILoadable
{
    public static PhysicsSolver Solver { get; private set; }

    public void Load(Mod mod)
    {
        Solver = new PhysicsSolver();
        Solver.AddPhysicsPass(new SecondOrderIntegration());
    }

    public void Unload() => Solver = null;
}
