using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpaceEventMod.Core.GameObjects.FirmamentSea;

public struct FirmamentSea
{
    public FirmamentSea(Vector2 position, float nodeWidth, int chunkSize, int chunks)
    {
        this.Position = position;
        this.NodeWidth = nodeWidth;
        this.ChunkSize = chunkSize;

        var nodes = new HookeSpring[chunks, chunkSize];

        for (int outer = 0; outer < nodes.GetLength(0); outer++)
        {
            for (int inner = 0; inner < nodes.GetLength(1); inner++)
                nodes[outer, inner] = new HookeSpring();
        }

        this.Springs = nodes;

        var sineOffsets = new List<float>();
        var sineAmplitudes = new List<float>();
        var sineStretches = new List<float>();
        var offsetStretches = new List<float>();

        for (var i = 0; i < 7; i++)
        {
            sineOffsets.Add(-1 + 2 * Main.rand.NextFloat());
            sineAmplitudes.Add(5f * Main.rand.NextFloat());
            sineStretches.Add(0.05f * Main.rand.NextFloat());
            offsetStretches.Add(10f * Main.rand.NextFloat());
        }

        this.SineOffsets = sineOffsets.ToArray();
        this.SineAmplitudes = sineAmplitudes.ToArray();
        this.SineStretches = sineStretches.ToArray();
        this.OffsetStretches = offsetStretches.ToArray();

        this.Active = true;
    }

    public bool Active;
    public Vector2 Position;
    public float NodeWidth;
    public int ChunkSize;

    public HookeSpring[,] Springs;

    public float[] SineOffsets;
    public float[] SineAmplitudes;
    public float[] SineStretches;
    public float[] OffsetStretches;

    public float OverlapSines(float x)
    {
        float result = 0;

        for (var i = 0; i < 7; i++)
        {
            result += SineOffsets[i] + SineAmplitudes[i] * MathF.Sin(x * SineStretches[i] + Main.GlobalTimeWrappedHourly * OffsetStretches[i]);
        }

        return result;
    }
}