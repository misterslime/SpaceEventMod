using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IDynamicMotion
{
    public Vector2Dynamics SecondOrderSolver { get; set; }
    public Vector2 TargetPosition { get; set; }
}
