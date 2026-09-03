using Microsoft.Xna.Framework;

namespace SpaceEventMod.Common.SDFs;

/// <summary>
/// Interface that signifies a type is a 2D shape with a signed distance function.
/// </summary>
internal interface ISignedDistance2D
{
    public Vector3 GetSignedDistance(Vector2 position);
}

