#include "SdfCircle.fxh"

sampler metaballTarget : register(s0);

uniform float4 metaballData[MAX_METABALLS];
uniform int metaballCount;
uniform float smoothness;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float3 unpack(float4 color) 
{
    float2 gradient = 2.0 * (color.rg - 0.5);
    float distance = (color.a > 0.) ? -color.b : (1.0 / color.b) - 1.0;
    return float3(distance, gradient);
}

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float4 sample = tex2D(metaballTarget, uv);

    float2 worldPixelPosition = screenPos + uv * worldViewDimensions;

    float3 total = unpack(sample);

    return getSdfColor(total, metaballData, metaballCount, smoothness, worldPixelPosition);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}