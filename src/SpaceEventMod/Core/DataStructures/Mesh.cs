using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpaceEventMod.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace SpaceEventMod.Core.DataStructures;

public struct Mesh
{
    private readonly VertexPositionColor[] _vertices;

    private int _positionInBuffer = 0;

    private readonly PrimitiveType _type;

    public ReadOnlySpan<VertexPositionColor> Vertices { get { return _vertices.AsSpan()[0.._positionInBuffer]; } }

    public PrimitiveType Type { get { return _type; } }

    public Mesh(PrimitiveType primitiveType, int bufferSize = 500)
    {
        _type = primitiveType;
        _vertices = new VertexPositionColor[bufferSize];

        if (primitiveType == PrimitiveType.LineStrip ||
            primitiveType == PrimitiveType.TriangleStrip)
        {
            throw new NotSupportedException
                ("The specified primitiveType is not supported by PrimitiveBatch.");
        }
    }

    public Mesh(PrimitiveType primitiveType, Effect effect, int bufferSize = 500) 
    {
        _type = primitiveType;
        _vertices = new VertexPositionColor[bufferSize];

        if (primitiveType == PrimitiveType.LineStrip ||
            primitiveType == PrimitiveType.TriangleStrip)
        {
            throw new NotSupportedException
                ("The specified primitiveType is not supported by PrimitiveBatch.");
        }
    }

    public Mesh AddVertex(Vector2 vertex, Color color)
    {
        _vertices[_positionInBuffer].Position = new Vector3(vertex, 0);
        _vertices[_positionInBuffer].Color = color;

        _positionInBuffer++;

        return this;
    }

    public Mesh AddVertex(Vector3 vertex, Color color)
    {
        _vertices[_positionInBuffer].Position = vertex;
        _vertices[_positionInBuffer].Color = color;

        _positionInBuffer++;

        return this;
    }

    public int NumVertsPerPrimitive()
    {
        int numVertsPerPrimitive;
        switch (_type)
        {
            case PrimitiveType.LineList:
                numVertsPerPrimitive = 2;
                break;
            case PrimitiveType.TriangleList:
                numVertsPerPrimitive = 3;
                break;
            default:
                throw new InvalidOperationException("primitive is not valid");
        }
        return numVertsPerPrimitive;
    }
}
