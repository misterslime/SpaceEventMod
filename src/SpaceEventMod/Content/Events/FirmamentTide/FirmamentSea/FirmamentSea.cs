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

namespace SpaceEventMod.Content.Events.FirmamentTide.FirmamentSea;

public struct SeaPosition(int left)
{
    public int Left = left;
    public Kinematics<float> Height = new Kinematics<float>(0);
}

public struct FirmamentSea
{
    public FirmamentSea(float nodeWidth, int chunkSize, int chunks)
    {
        NodeWidth = nodeWidth;
        ChunkSize = chunkSize;
        Chunks = chunks;

        var chunkWorldSize = (int)(nodeWidth * chunkSize);

        SeaPos = new SeaPosition((int)(Main.LocalPlayer.Center.X / chunkWorldSize) - chunks / 2);

        var springs = new Spring[chunks][];

        for (var i = 0; i < springs.Length; i++)
            springs[i] = new Spring[chunkSize];

        Springs = springs;

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

        SineOffsets = sineOffsets.ToArray();
        SineAmplitudes = sineAmplitudes.ToArray();
        SineStretches = sineStretches.ToArray();
        OffsetStretches = offsetStretches.ToArray();

        Active = true;
        Despawning = false;
    }

    public bool Despawning = true;
    public bool Active = false;
    public SeaPosition SeaPos;
    public float NodeWidth;
    public int ChunkSize;
    public int Chunks;

    public Spring[][] Springs;

    public float[] SineOffsets;
    public float[] SineAmplitudes;
    public float[] SineStretches;
    public float[] OffsetStretches;

    public bool CanSpawnThings { get => !Despawning && Math.Abs(SeaPos.Height.Velocity) < 1; }

    public Vector2 Position { get => new Vector2(SeaPos.Left * ChunkSize * NodeWidth, SeaPos.Height.Position); }

    public float OverlapSines(float x)
    {
        float result = 0;

        for (var i = 0; i < 7; i++)
            result += SineOffsets[i] + SineAmplitudes[i] * MathF.Sin(x * SineStretches[i] + Main.GlobalTimeWrappedHourly * OffsetStretches[i]);

        return result;
    }
}