#include "SdfCircle.fxh"

uniform float4 metaballData[MAX_METABALLS];
uniform int metaballCount;
uniform float smoothness;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float2 worldPixelPosition = screenPos + uv * worldViewDimensions;

    float3 total = float3(99999.0, 0., 0.);

    return getSdfColor(total, metaballData, metaballCount, smoothness, worldPixelPosition);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}