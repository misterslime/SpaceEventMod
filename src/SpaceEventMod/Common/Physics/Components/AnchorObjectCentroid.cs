using SpaceEventMod.Common.Physics.Attributes;
using SpaceEventMod.Common.Physics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Common.Physics.Components;

// if anchorObject is true, the component's object will be moved to the PhysicsShape's centroid
// if false, the centroid will be moved to the PhysicsObject's center
[Needs(typeof(PhysicsShape))]
internal struct AnchorObjectCentroid(bool anchorObject) : IComponent
{
    public bool AnchorObject { get; set; } = anchorObject;
}
