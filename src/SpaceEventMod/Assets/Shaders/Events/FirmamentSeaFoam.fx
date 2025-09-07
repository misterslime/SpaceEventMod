#include "../EdgeDetection.fxh"
#include "../Pixelate.fxh"
#include "../Math.fxh"

sampler seaTarget : register(s0);

texture noise;
sampler noiseSampler = sampler_state
{
    Texture = (noise);
};

texture palette;
sampler paletteSampler = sampler_state
{
    Texture = (palette);
    Filter = MIN_MAG_MIP_POINT;
    AddressU = clamp;
    AddressV = clamp;
};

float2 screenSize;
float2 screenWorldPosition;
float globalTime;

float4 edgeColor1;
float4 edgeColor2;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float2 pixelSize = 2. / screenSize;

    //pixelate
    float2 screenWorldCoords = pixelate((screenPosition + screenWorldPosition) / screenSize, pixelSize);
    float2 noiseCoords = screenWorldCoords + float2(globalTime * 0.01, globalTime * 0.005);

    coords = pixelate(coords, pixelSize);

    // noise sampling
    float sample = (tex2D(noiseSampler, noiseCoords).g + tex2D(seaTarget, coords).g) * 0.5;
    float4 paletteColor = step(0.5, sample) * tex2D(paletteSampler, float2(sample, 0.));

    // edge detection
    float edge = edgeDetection(seaTarget, coords, pixelSize) / 15.;

    // abomination
    float4 edgeColor = lerp(edgeColor1, edgeColor2, pow(sin(noiseCoords.x * TAU * 2.5), 2));

    return (edge > 0.) ? edgeColor : paletteColor;
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}