using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Components.SoftBodies;

internal struct GasFilledSoftBody(float desiredArea, float currentArea, float scaleFactor) : IComponent
{
    public float DesiredArea { get; init; } = desiredArea;
    public float ScaleFactor { get; init; } = scaleFactor;
}
