using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceEventMod.Common.DataStructures;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SpaceEventMod.Common.Graphics;
public struct Commands()
{
    public List<CommandType> Types = [];
    public List<int> Datas = [];

    public readonly int Count => Types.Count;
    public readonly void Add(CommandType type, int data)
    {
        Types.Add(type);
        Datas.Add(data);
    }

    public readonly void AddRange(in Commands commands)
    {
        Types.AddRange(commands.Types);
        Datas.AddRange(commands.Datas);
    }

    public readonly void Clear()
    {
        Types.Clear();
        Datas.Clear();
    }
}

public enum CommandType : byte
{
    DrawTrail,
    DrawSprite,
    DrawMesh,

    Begin,
    End,

    ApplyEffect,
    EffectParams,

    SetBlendState,
    SetTexture,
    SetSamplerState,
}

public record struct SamplerStateData(int Index, SamplerState State);
public record struct TextureData(int Index, Texture2D Texture);

public record struct DrawMeshData(
    int VerticesIndex,
    int VertexCount,
    PrimitiveType PrimitiveType,
    int VerticesPerPrimitive,
    int EffectDataIndex
);

public record struct DrawSpriteData(
    Texture2D Texture,
    Color Color,
    Matrix Matrix,
    Vector4 Source,
    Effect Effect
);

public record struct DrawTrailData(
    int PositionsIndex,
    int PositionCount,
    Func<float, float> Width,
    Func<float, Color> Color,
    int EffectDataIndex
);

public record struct BeginData(float Scale);
public record struct EffectData(Effect Effect, int ParameterIndex, int ParameterCount);
public record struct EffectParameterData(int Index, ParameterValue Value);
