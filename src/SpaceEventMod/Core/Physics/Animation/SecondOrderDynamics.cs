using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.Physics.Animation;

internal class SecondOrderDynamics : ILoadable
{
    public static PhysicsSolver Solver { get; private set; }

    public void Load(Mod mod) => Solver = new PhysicsSolver(Integrators.SecondOrderIntegration);

    public void Unload() => Solver = null;
}
