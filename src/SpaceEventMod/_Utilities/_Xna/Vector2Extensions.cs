using Microsoft.Xna.Framework;
using System;

namespace SpaceEventMod;

public static class Vector2Extensions
{
    public static Vector3 ToVector3(this Vector2 vector, float z = 0f) => new Vector3(vector.X, vector.Y, z);

    /// <summary>
    /// Cool method for figuring out if a circle is colliding with a line segment.
    /// From this stackoverflow answer: https://stackoverflow.com/a/1079478
    /// </summary>
    /// <param name="vector">Circle center.</param>
    /// <param name="A">Point A of the line segment.</param>
    /// <param name="B">Point B of the line segment.</param>
    /// <returns>Returns the distance from line segment AB to point C</returns>
    public static float DistanceSegmentToPoint(this Vector2 vector, Vector2 A, Vector2 B)
    {
        // Compute vectors AC and AB
        var AC = vector - A;
        var AB = B - A;

        // Get point D by taking the projection of AC onto AB then adding the offset of A
        var D = AC.Project(AB) + A;

        var AD = D - A;

        // D might not be on AB so calculate k of D down AB (aka solve AD = k * AB)
        // We can use either component, but choose larger value to reduce the chance of dividing by zero
        var k = MathF.Abs(AB.X) > MathF.Abs(AB.Y) ? AD.X / AB.X : AD.Y / AB.Y;

        // Check if D is off either end of the line segment
        if (k <= 0.0)
            return MathF.Sqrt(vector.Hypot2(A));
        else if (k >= 1.0)
            return MathF.Sqrt(vector.Hypot2(B));

        return MathF.Sqrt(vector.Hypot2(D));
    }

    // Function for projecting some vector A onto B
    public static Vector2 Project(this Vector2 A, Vector2 B)
    {
        var k = Vector2.Dot(A, B) / Vector2.Dot(B, B);
        return new Vector2(k * B.X, k * B.Y);
    }

    public static float Hypot2(this Vector2 a, Vector2 b) => Vector2.Dot(a - b, a - b);

    /// <summary>
    /// Function that gets the angle you'd need to hit a target given your projectile is affected by gravity.
    /// 
    /// Because this was math'd in desmos where down is negative,
    /// you'll have to ensure that you flip the sign of the target vector's y component.
    /// Math was done by @azaliesthyl on discord :D
    /// </summary>
    /// <param name="target">Vector from launch to target.</param>
    /// <param name="throwingVelocity">Velocity the projectile is shot at.</param>
    /// <param name="gravity">Acceleration due to gravity.</param>
    /// <returns>The angle of the velocity, returns null if it cannot hit.</returns>
    public static float? GetArtilleryAngle(this Vector2 target, float throwingVelocity, float gravity)
    {
        float theta = 0;
        var A = (gravity * MathF.Pow(target.X, 2)) / (2 * MathF.Pow(throwingVelocity, 2));

        if (-MathF.Sqrt(MathF.Pow(target.X, 2) + MathF.Pow(target.Y, 2)) <= (gravity / MathF.Pow(throwingVelocity, 2)) * MathF.Pow(target.X, 2) - target.Y)
        {
            if (0 <= target.X)
                theta = MathF.Atan((-target.X + MathF.Sqrt(MathF.Pow(target.X, 2) - (4 * A * (A - target.Y)))) / (2 * A));
            if (0 > target.X)
                theta = MathF.PI + MathF.Atan((-target.X - MathF.Sqrt(MathF.Pow(target.X, 2) - (4 * A * (A - target.Y)))) / (2 * A));

            return theta;
        }

        return null;
    }

    public static Vector2 Sign(this Vector2 v) => new Vector2(MathF.Sign(v.X), MathF.Sign(v.Y));

    public static Vector2 Abs(this Vector2 v) => new Vector2(MathF.Abs(v.X), MathF.Abs(v.Y));
}
