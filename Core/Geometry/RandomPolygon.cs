using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Utilities;

namespace SpaceEventMod.Core.Geometry;

public class RandomPolygon
{
    /// <summary>
    /// Start with the center of the polygon, then create the polygon by getting points on a circle around the center.
    /// Random noise is added by varying the angle between points, and by varying the distance of each point from the center.
    /// </summary>
    /// <returns></returns>
    public static Polygon GeneratePolygon(int seed, Vector2 center, float minRadius, float maxRadius, float irregularity, int numberOfVertices)
    {
        if (irregularity < 0 || irregularity > 1)
            throw new ArgumentOutOfRangeException(nameof(irregularity), " must be between 0 and 1.");

        UnifiedRandom random = new UnifiedRandom(seed);

        irregularity *= MathHelper.TwoPi / numberOfVertices;
        List<float> angles = RandomAngles(random, numberOfVertices, irregularity);

        List<Vector2> points = new List<Vector2>();
        float angle = random.NextFloat(0, MathHelper.TwoPi);

        for (int i = 0; i < numberOfVertices; i++)
        {
            float radius = random.NextFloat(minRadius, maxRadius);
            points.Add(new Vector2(center.X + radius * MathF.Cos(angle), center.Y + radius * MathF.Sin(angle)));
            angle += angles[i];
        }

        return new Polygon([.. points]);
    }

    /// <summary>
    /// Generates the division of a circle in random angles.
    /// </summary>
    /// <returns>A list of the random angles.</returns>
    public static List<float> RandomAngles(UnifiedRandom random, int steps, float irregularity)
    {
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
