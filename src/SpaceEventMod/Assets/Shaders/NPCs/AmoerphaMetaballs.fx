#include "../SignedDistanceFunctions.fxh"

#define MAX_METABALLS 64

struct metaball {
    float2 position; // world coords
    float radius;    // world coords
    float padding;
};

uniform float4 metaballData[MAX_METABALLS];
uniform int metaballCount;
uniform float smoothness;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float circ(float r, float pos)
{
    return sqrt((r * r) - (pos * pos));
}

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
    float3 total = float3(99999.0, 0., 0.);

    float2 worldPixelPosition = screenPos + uv * worldViewDimensions;

    for (int i = 0; i < metaballCount; i++) {
        metaball ball = (metaball)metaballData[i]; 

        float3 dist = sdgCircle(worldPixelPosition, ball.position, ball.radius);

        total = smin(total, dist, smoothness);
    }

    // laziest fix in history
    float2 d = float2(abs(clamp(1.0 + total.x, 0.0001, 1.0)), 0.0);

    d.y = abs(circ(1.0, d.x));

    float2 g = total.yz * d.x;
    
    
	// coloring
    float b = 0.5 * d.y;
    float3 col = float3(0.5+0.5*g.x, 0.5+0.5*g.y, 0.5 + b);

    return float4(col.x, col.y, col.z, 1.0-step(0.0,total.x));
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}