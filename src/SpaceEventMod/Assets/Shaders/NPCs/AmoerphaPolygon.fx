#include "../SignedDistanceFunctions.fxh"

uniform float2 vertexData[MAX_VERTICES];
uniform int vertexCount;
uniform float smoothness;
uniform float radius;
uniform float roundness;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float circ(float r, float pos)
{
    return sqrt((r * r) - (pos * pos));
}

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
    float2 worldPixelPosition = screenPos + uv * worldViewDimensions;

    float3 dg = sdgPolygon(worldPixelPosition, vertexData, vertexCount);
    float d = dg.x - radius;
    float2 g = dg.yz;

    float wawa = 1.0 + (d / roundness);
    
    float2 the = float2(clamp(wawa, 0.0001, 1.0), 0.0);

    the.y = abs(circ(1.0, the.x));

    g *= the.x;
    
    float b = 0.5 * the.y;

    float3 col = float3(0.5+0.5*g.x, 0.5+0.5*g.y, 0.5 + b);

    return float4(col.x, col.y, col.z, 1.0-step(0.0,d));
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}