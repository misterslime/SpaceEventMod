// ripped from here
// https://www.shadertoy.com/view/fsV3R3

#include "Math.fxh"

static const int SAMPLES = 25;
static const float SIGMA = float(SAMPLES) * 0.25;

// we don't need to recalculate these every time
static const float SIGMA2 = 2. * SIGMA * SIGMA;
static const float PISIGMA2 = PI * SIGMA2;

float gaussian(float2 i) 
{
    float top = exp(-((i.x * i.x) + (i.y * i.y)) / SIGMA2);
    float bot = PISIGMA2;
    return top / bot;
}

float3 blur(sampler sp, float2 uv, float2 scale) 
{
    float2 offset = float2(0., 0.);
    float weight = gaussian(offset);
    float3 col = tex2D(sp, uv).rgb * weight;
    float accum = weight;
    
    // we need to use x <= SAMPLES / 2
    // to ensure symmetry
    for (int x = 0; x <= SAMPLES / 2; ++x) {
        for (int y = 1; y <= SAMPLES / 2; ++y) {
            offset = float2(x, y);
            weight = gaussian(offset);
            col += tex2D(sp, uv + scale * offset).rgb * weight;
            accum += weight;

            // since values are symmetrical
            // we can re-use the "weight" value, saving 3 function calls

            col += tex2D(sp, uv - scale * offset).rgb * weight;
            accum += weight;

            offset = float2(-y, x);
            col += tex2D(sp, uv + scale * offset).rgb * weight;
            accum += weight;

            col += tex2D(sp, uv - scale * offset).rgb * weight;
            accum += weight;
        }
    }
    
    return col / accum;
}
