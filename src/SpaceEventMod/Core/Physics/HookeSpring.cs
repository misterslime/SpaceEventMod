using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod.Core.Physics;

public struct HookeSpring()
{
    public float Height, Velocity = 0;
}

public static class HookeUtilities
{
    public static HookeSpring Update(this HookeSpring spring, float dampening, float tension)
    {
        float acceleration = (-tension * spring.Height) - (dampening * spring.Velocity);

        HookeSpring newSpring = spring;
        newSpring.Height += spring.Velocity;
        newSpring.Velocity += acceleration;

        return newSpring;
    }

    public static HookeSpring[] UpdateArray(this HookeSpring[] springs, float dampening, float tension)
    {
        HookeSpring[] newArray = springs;

        for (int i = 0; i < newArray.Length; i++)
            newArray[i] = springs[i].Update(dampening, tension);

        return newArray;
    }

    public static HookeSpring[] PropagateWaves(this HookeSpring[] springs, float spread, int passes = 8)
    {
        HookeSpring[] newArray = springs;

        float clampedSpread = MathHelper.Clamp(spread, 0f, 0.5f);

        float[] leftDeltas = new float[springs.Length];
        float[] rightDeltas = new float[springs.Length];

        // do some passes where springs pull on their neighbours
        for (int j = 0; j < passes; j++)
        {
            for (int i = 0; i < springs.Length; i++)
            {
                if (i > 0)
                {
                    leftDeltas[i] = clampedSpread * (springs[i].Height - springs[i - 1].Height);
                    newArray[i - 1].Velocity += leftDeltas[i];
                }

                if (i < springs.Length - 1)
                {
                    rightDeltas[i] = clampedSpread * (springs[i].Height - springs[i + 1].Height);
                    newArray[i + 1].Velocity += rightDeltas[i];
                }
            }

            for (int i = 0; i < springs.Length; i++)
            {
                if (i > 0)
                    newArray[i - 1].Height += leftDeltas[i];
                if (i < springs.Length - 1)
                    newArray[i + 1].Height += rightDeltas[i];
            }
        }

        return newArray;
    }
}
