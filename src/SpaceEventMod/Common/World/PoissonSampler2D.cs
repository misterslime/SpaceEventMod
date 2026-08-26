using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Common.World;

internal class PoissonSampler2D
{
    private List<Point> _seedDimensions;
    private List<Vector2> _seedPoints;

    private Vector2 _sampleRegionSize;
    private List<Vector2> _samples;
    private int[,] _grid;
    private float _cellSize;
    private float _radius;

    public List<Vector2> Samples { get => _samples; }
    public int[,] Grid { get => _grid; }

    public PoissonSampler2D(float radius, Vector2 sampleRegionSize, FastNoiseLite noise, List<Point> seedDimensions, List<Vector2> seedPoints, int tries = 30)
    {
        _seedDimensions = seedDimensions;
        _seedPoints = seedPoints;

        _sampleRegionSize = sampleRegionSize;
        _radius = radius;
        _cellSize = radius / MathF.Sqrt(2);
        _grid = new int[(int)MathF.Ceiling(sampleRegionSize.X / _cellSize), (int)MathF.Ceiling(sampleRegionSize.Y / _cellSize)];
        _samples = new List<Vector2>();

        List<Vector2> spawnPoints = new List<Vector2>();
        spawnPoints.AddRange(_seedPoints);

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = WorldGen.genRand.Next(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < tries; i++)
            {
                Vector2 direction = WorldGen.genRand.NextVector2Unit();

                float distance = WorldGen.genRand.NextFloat(_radius, 2 * _radius);
                distance *= (1 - MathF.Abs(noise.GetNoise(spawnCentre.X, spawnCentre.Y))) * 0.25f + 1f;

                Vector2 candidate = spawnCentre + direction * WorldGen.genRand.NextFloat(_radius, 2 * _radius);

                if (IsValid(candidate, noise))
                {
                    _samples.Add(candidate);
                    spawnPoints.Add(candidate);
                    _grid[(int)(candidate.X / _cellSize), (int)(candidate.Y / _cellSize)] = _samples.Count;
                    candidateAccepted = true;
                    break;
                }
            }

            if (!candidateAccepted)
                spawnPoints.RemoveAt(spawnIndex);
        }

        // make sure the grid functions
        for (int i = 0; i < _samples.Count; i++)
        {
            Vector2 sample = _samples[i];

            _grid[(int)(sample.X / _cellSize), (int)(sample.Y / _cellSize)] = i;
        }
    }

    private bool IsValid(Vector2 candidate, FastNoiseLite noise)
    {
        if (candidate.X < 0 || candidate.X >= _sampleRegionSize.X || candidate.Y < 0 || candidate.Y >= _sampleRegionSize.Y)
            return false;

        float total = 99999f;

        for (int i = 0; i < _seedPoints.Count; i++)
        {
            Vector2 relativePosition = candidate - _seedPoints[i];

            float dist = SignedDistanceFunctions.EllipseSDF(relativePosition, _seedDimensions[i].ToVector2());

            total = MathF.Min(total, dist);
        }

        float noiseSample = noise.GetNoise(candidate.X, candidate.Y);
        float scale = (int)((Main.maxTilesX / 4200f) * 25);

        if (total + scale * noiseSample >= 0)
            return false;

        int cellX = (int)(candidate.X / _cellSize);
        int cellY = (int)(candidate.Y / _cellSize);

        int searchStartX = Math.Max(0, cellX - 2);
        int searchEndX = Math.Min(_grid.GetLength(0) - 1, cellX + 2);
        int searchStartY = Math.Max(0, cellY - 2);
        int searchEndY = Math.Min(_grid.GetLength(1) - 1, cellY + 2);

        for (int i = searchStartX; i <= searchEndX; i++)
        {
            for (int j = searchStartY; j <= searchEndY; j++)
            {
                int pointIndex = _grid[i, j] - 1;

                if (pointIndex != -1)
                {
                    float sqrDist = (candidate - _samples[pointIndex]).LengthSquared();

                    if (sqrDist < _radius * _radius)
                        return false;
                }
            }
        }

        return true;
    }
}
