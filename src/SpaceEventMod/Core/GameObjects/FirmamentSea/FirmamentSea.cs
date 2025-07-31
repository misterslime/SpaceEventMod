using Microsoft.Xna.Framework;
using SpaceEventMod.Core.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace SpaceEventMod.Core.GameObjects.FirmamentSea;

public struct FirmamentSea
{
    public Vector2 Position;
    public float NodeWidth;
    public bool Active;
    public float Spread;

    public HookeSpring[] Nodes;

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
