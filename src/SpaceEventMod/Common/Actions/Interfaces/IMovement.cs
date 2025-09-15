using Microsoft.Xna.Framework;

namespace SpaceEventMod.Common.Actions.Interfaces;

public interface IMovement
{
    /// <summary>
    /// Allows me to decouple movement code from state behaviour code >:3
    /// </summary>
    /// <param name="motionVector">Direction/velocity/you decide of motion.</param>
    /// <param name="arguments">Extra parameters to pass into the function.</param>
    public void EntityMovement(Vector2 motionVector, params float[] arguments);
}
