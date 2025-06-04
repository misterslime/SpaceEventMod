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
    /// <param name="seed">Seed to use for the polygon generation.</param>
    /// <param name="center">Position of the polygon's center.</param>
    /// <param name="minRadius">How close the points can be to <paramref name="center"/>.</param>
    /// <param name="maxRadius">How far the points can be from <paramref name="center"/>.</param>
    /// <param name="irregularity">How different it can be from a regular polygon.</param>
    /// <param name="numberOfVertices">How many vertices the final polygon will have.</param>
    /// <returns>A randomly generated <see cref="Polygon"/>.</returns>
    public static Polygon GeneratePolygon(int seed, Vector2 center, float minRadius, float maxRadius, float irregularity, int numberOfVertices)
    {
        if (irregularity < 0 || irregularity > 1)
            throw new ArgumentOutOfRangeException(nameof(irregularity), " must be between 0 and 1.");

        var random = new UnifiedRandom(seed);

        irregularity *= MathHelper.TwoPi / numberOfVertices;
        var angles = RandomAngles(random, numberOfVertices, irregularity);

        var points = new List<Vector2>();
        var angle = random.NextFloat(0, MathHelper.TwoPi);

        for (var i = 0; i < numberOfVertices; i++)
        {
            var radius = random.NextFloat(minRadius, maxRadius);
            points.Add(new Vector2(center.X + radius * MathF.Cos(angle), center.Y + radius * MathF.Sin(angle)));
            angle += angles[i];
        }

        return new Polygon([.. points]);
    }

    /// <summary>
    /// Generates the division of a circle in random angles.
    /// </summary>
    /// <param name="random"><see cref="UnifiedRandom"/> to use for random angle generation.</param>
    /// <param name="steps">Number of angles to be generated.</param>
    /// <param name="irregularity">How different the angles can be from each other.</param>
    /// <returns>A list of the random angles.</returns>
    public static List<float> RandomAngles(UnifiedRandom random, int steps, float irregularity)
    {
        var angles = new List<float>();
        var lower = MathHelper.TwoPi / steps - irregularity;
        var upper = MathHelper.TwoPi / steps + irregularity;
        float cumulativeSum = 0;

        for (var i = 0; i < steps; i++)
        {
            var angle = random.NextFloat(lower, upper);
            angles.Add(angle);
            cumulativeSum += angle;
        }

        cumulativeSum /= MathHelper.TwoPi;

        for (var i = 0; i < steps; i++)
            angles[i] /= cumulativeSum;

        return angles;
    }
}