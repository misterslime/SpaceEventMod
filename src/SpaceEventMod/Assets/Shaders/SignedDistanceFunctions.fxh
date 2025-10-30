// .x = f(p)
// .y = ∂f(p)/∂x
// .z = ∂f(p)/∂y
// .yz = ∇f(p) with ‖∇f(p)‖ = 1

#include "Math.fxh"

#define MAX_VERTICES 64

float3 sdgPolygon( in float2 worldPixelCoords, in float2 vertices[MAX_VERTICES], in int num)
{
    int numMinusOne = num - 1;

    float d = dot(worldPixelCoords-vertices[0],worldPixelCoords-vertices[0]);
    float l = 1.0;

    float gs = cro(vertices[0]-vertices[numMinusOne],vertices[1]-vertices[0]);
    float4 res;
    
    for( int i=0, j=1; i<num; i++, j = (j + 1) % num )
    {
        // distance
        float2  e = vertices[j] - vertices[i];
        float2  w = worldPixelCoords - vertices[i];
        float2  q = w-e*clamp(dot(w,e)/dot(e,e),0.0,1.0);
        d = min( d, dot(q,q) );
        float s = gs*cro(w,e);
        
        bool3 cond = bool3( worldPixelCoords.y>=vertices[i].y, 
                            worldPixelCoords.y <vertices[j].y, 
                            e.x*w.y>e.y*w.x );
                            
        if( all(cond) || all(!cond) ) l=-l;

        res = (i == 0) ? float4(d,q,s) : float4( (d<res.x) ? float3(d,q) : res.xyz,
                                                 (s>res.w) ?        s    : res.w );
    }
    
    // distance and sign
    d = sqrt(res.x)*sign(l);
    return float3(d, res.yz / d);
}

float3 sdgCircle(in float2 worldPixelCoords, in float2 position, in float radius) 
{
    float2 relativePosition = worldPixelCoords - position;
    float dist = length(relativePosition);
    float signedDistance = (dist / radius) - 1.0;
    return float3(signedDistance, relativePosition.x / dist, relativePosition.y / dist);
}

float3 smin(float3 a, float3 b, float k) {
    k *= 4.0;
    float h = max(k-abs(a.x-b.x),0.0)/(2.0*k);
    float2 gradient = lerp(a.yz,b.yz,(a.x<b.x)?h:1.0-h);
    return float3(min(a.x,b.x)-h*h*k, gradient.x, gradient.y);
}