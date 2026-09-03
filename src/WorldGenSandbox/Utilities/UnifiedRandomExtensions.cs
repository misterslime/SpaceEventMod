using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Utilities;

namespace WorldGenSandbox.Utilities;

internal static class UnifiedRandomExtensions
{
    /// <summary>
    /// Generates the division of a circle in random angles.
    /// </summary>
    /// <returns>A list of the random angles.</returns>
    public static List<float> NextRandomAngles(this UnifiedRandom random, int steps, float irregularity)
    {
        if (irregularity < 0 || irregularity > 1)
            throw new ArgumentOutOfRangeException(nameof(irregularity), " must be between 0 and 1.");

        if (steps < 1)
            throw new ArgumentOutOfRangeException(nameof(steps), " must be more than 0.");

        irregularity *= MathHelper.TwoPi / steps;

        List<float> angles = new List<float>();
        float lower = MathHelper.TwoPi / steps - irregularity;
        float upper = MathHelper.TwoPi / steps + irregularity;
        float cumulativeSum = 0;

        for (int i = 0; i < steps; i++)
        {
            float angle = random.NextFloat(lower, upper);
            angles.Add(angle);
            cumulativeSum += angle;
        }

        cumulativeSum /= MathHelper.TwoPi;

        for (int i = 0; i < steps; i++)
            angles[i] /= cumulativeSum;

        return angles;
    }
}
