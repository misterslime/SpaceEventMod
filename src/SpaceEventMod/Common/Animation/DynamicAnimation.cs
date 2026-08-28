using Microsoft.Xna.Framework;
using SpaceEventMod.Common.Physics;
using SpaceEventMod.Common.Physics.Components.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Common.Animation;

internal struct AnimationParameters
{
    public float _w { get; init; }
    public float _z { get; init; }
    public float _d { get; init; }

    public float k1 { get; init; }
    public float k2 { get; init; }
    public float k3 { get; init; }

    /// <summary>
    /// Parameters for a second order dynamic animation/easing functon to be integrated over.
    /// </summary>
    /// <param name="frequency"></param>
    /// <param name="dampening"></param>
    /// <param name="anticipation"></param>
    public AnimationParameters(float frequency, float dampening, float anticipation)
    {
        float scaledFrequency = frequency / 60f;

        _w = 2 * MathF.PI * scaledFrequency;
        _z = dampening;
        _d = _w * MathF.Sqrt(MathF.Abs(dampening * dampening - 1));
        k1 = dampening / (MathF.PI * scaledFrequency);
        k2 = 1 / (_w * _w);
        k3 = anticipation * dampening / _w;
    }
}

/// <summary>
/// Class with a bunch of helper methods for dynamic animations.
/// To-do: make this able to load animation parameter sets from json//hjson/data files
/// </summary>
internal static class DynamicAnimation
{
    private const float DELTA_TIME = 1 / 60; // 60 fps

    /// <summary>
    /// Integrates the current position and velocity of an entity to target <paramref name="inputPosition"/>.
    /// </summary>
    /// <param name="entity">The entity being integrated upon.</param>
    /// <param name="parameters">Values used to integrate the function.</param>
    /// <param name="inputPosition">Target position.</param>
    public static void Integrate(
        this Entity entity,
        AnimationParameters parameters,
        Vector2 inputPosition)
    {

        /*float k1 = parameters.GetK1;
        float k2 = parameters.GetK2;
        float k3 = parameters.GetK3;

        var k2Constrained = MathF.Max(k2, 1.1f * (DELTA_TIME * DELTA_TIME * 0.25f + DELTA_TIME * k1 * 0.5f));
        Vector2 deltaInput = (k3 + 1) * inputPosition - k3 * inputPosition;
        Vector2 deltaCurrent = k1 * entity.oldPosition - (k1 + 1) * entity.position;
        Vector2 acceleration = (deltaInput + deltaCurrent) / k2;

        acceleration *= DELTA_TIME * DELTA_TIME;

        var newPosition = 2 * entity.position - entity.oldPosition + acceleration;
        entity.oldPosition = entity.position;
        entity.position = newPosition;

        entity.velocity = Vector2.Zero;


        return;*/

        var deltaTime = 1.0f;

        //var currentInput = inputPosition;
        //var nextPosition = entity.position;

        var velocityEstimate = (inputPosition - entity.oldPosition) / deltaTime;
        entity.oldPosition = inputPosition;

        float k1Stable = parameters.k1;
        float k2Stable = parameters.k2;

        if (parameters._w * deltaTime < parameters._z)
            k2Stable = MathF.Max(parameters.k2, MathF.Max(deltaTime * deltaTime * 0.5f + deltaTime * parameters.k1 * 0.5f, deltaTime * parameters.k1));
        else
        {
            float t1 = MathF.Exp(-parameters._z * parameters._w * deltaTime);
            float alpha = 2 * t1 * (parameters._z <= 1 ? MathF.Cos(deltaTime * parameters._d) : MathF.Cosh(deltaTime * parameters._d));
            float beta = t1 * t1;
            float t2 = deltaTime / (1 + beta - alpha);

            k1Stable = (1 - beta) * t2;
            k2Stable = deltaTime * t2;
        }

        //var k2Constrained = MathF.Max(parameters.k2, 1.1f * (deltaTime * deltaTime * 0.25f + deltaTime * parameters.k1 * 0.5f));
        //var k2Stable = MathF.Max(parameters.k2, MathF.Max(deltaTime * deltaTime * 0.5f + deltaTime * parameters.k1 * 0.5f, deltaTime * parameters.k1));
        //nextPosition += deltaTime * previousVelocity; // integrate position with velocity
        //previousVelocity += deltaTime * (inputPosition + parameters.k3 * velocityEstimate - nextPosition - k1Stable * entity.velocity) / k2Stable; // integrate velocity with acceleration

        //entity.position += deltaTime * entity.velocity;
        entity.velocity += deltaTime * (inputPosition + parameters.k3 * velocityEstimate - entity.position - k1Stable * entity.velocity) / k2Stable;
    }

    /// <summary>
    /// Integrates the current position and velocity to target <paramref name="inputPosition"/>.
    /// /// <inheritdoc cref="Integrate"/>
    /// </summary>
    /// <remarks>This specific function is meant to be used on floats.</remarks>
    /// <param name="kinematics">3 float velocity, position, and previous positions all packed into a vector.</param>
    /// <returns>The output position after integration.</returns>
    public static Vector3 Integrate(
        this AnimationParameters parameters,
        float inputPosition,
        Vector3 kinematics)
    {
        // X = position
        // Y = velocity
        // Z = previous position

        var currentInput = inputPosition;
        var inputVelocity = kinematics.Y;
        var nextPosition = kinematics.X;
        var previousVelocity = kinematics.Y;

        inputVelocity = (currentInput - kinematics.Z) / DELTA_TIME;
        kinematics.Z = inputPosition;

        var k2Constrained = MathF.Max(parameters.k2, 1.1f * (DELTA_TIME * DELTA_TIME * 0.25f + DELTA_TIME * parameters.k1 * 0.5f));
        nextPosition += DELTA_TIME * previousVelocity; // integrate position with velocity
        previousVelocity += DELTA_TIME * (inputPosition + parameters.k3 * inputVelocity - nextPosition - parameters.k1 * previousVelocity) / k2Constrained; // integrate velocity with acceleration

        kinematics.X = nextPosition;
        kinematics.Y = previousVelocity;

        return kinematics;
    }
}
