using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Behavior.StateMachines;
using SpaceEventMod.Core.Physics;


namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IDynamicStretch
{
    public Vector2Dynamics Stretching { get; set; }
    public Vector2 TargetStretching { get; set; }
}
