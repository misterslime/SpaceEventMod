#include "../SignedDistanceFunctions.fxh"

#define MAX_METABALLS 64

struct metaball 
{
    float2 position; // world coords
    float radius;    // world coords
    float padding;
};

float4 getSdfColor(in float3 total, in float4 metaballData[MAX_METABALLS], in int metaballCount, in float smoothness, in float2 worldPixelPosition)
{
    for (int i = 0; i < metaballCount; i++) 
    {
        metaball ball = (metaball)metaballData[i]; 

        float3 dist = sdgCircle(worldPixelPosition, ball.position, ball.radius);

        total = smin(total, dist, smoothness);
    }

    float2 gradient = total.yz * 0.5 + 0.5;

    // laziest fix in history
    if (total.x < 0.0) 
    {
        float distance = -total.x;
        distance = clamp(distance, 0., 1.);
        return float4(gradient.x, gradient.y, -total.x, 1.0);
    }
    
    return float4(gradient.x, gradient.y, 1.0 / (total.x + 1.0), 0.0);
}