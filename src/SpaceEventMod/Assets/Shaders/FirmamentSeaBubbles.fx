sampler seaTarget : register(s0);

texture bubbles;
sampler bubbleSampler = sampler_state
{
    Texture = (bubbles);
};

texture distortion;
sampler distortionSampler = sampler_state
{
    Texture = (distortion);
};

texture palette;
sampler paletteSampler = sampler_state
{
    Texture = (palette);
    Filter = MIN_MAG_MIP_POINT;
    AddressU = clamp;
    AddressV = clamp;
};

float3 sampleOffsetsAndScales[3];
float2 screenSize;
float2 screenWorldPosition;
float globalTime;
float gradientLength; // note this must never be 0 ever
float gradientStart;
float parallax;
float cutoff;

float GradientSample(float sample)
{
    float gradient = sample + gradientStart;
    return (gradient / gradientLength) - ((1 / gradientLength) - 1);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float2 screenWorldCoords = (screenPosition + (screenWorldPosition * parallax)) / screenSize;

    // distort
    float2 distortionCoords = float2(screenWorldCoords.x + globalTime * 0.07, screenWorldCoords.y + globalTime * 0.02);
    distortionCoords *= 0.5;
    float distortion = tex2D(distortionSampler, distortionCoords).r * 0.25;
    
    float4 seaTargetSample = tex2D(seaTarget, coords);

    // sample the bubble noise 3 times and overlay so they blend
    float2 bubbleCoords1 = (screenWorldCoords + distortion + globalTime * sampleOffsetsAndScales[0].xy) * sampleOffsetsAndScales[0].z;
    float2 bubbleCoords2 = (screenWorldCoords + distortion + globalTime * sampleOffsetsAndScales[1].xy) * sampleOffsetsAndScales[1].z;
    float2 bubbleCoords3 = (screenWorldCoords + distortion + globalTime * sampleOffsetsAndScales[2].xy) * sampleOffsetsAndScales[2].z;

    float4 bubbleSample = (tex2D(bubbleSampler, bubbleCoords1) + tex2D(bubbleSampler, bubbleCoords2) + tex2D(bubbleSampler, bubbleCoords3)) / 3;

    // render target shows a progress bar of sorts on how high in the sea the colors are
    float gradientSample = GradientSample(seaTargetSample.r);
    float sample = bubbleSample.r * gradientSample;
    
    float4 paletteColor = tex2D(paletteSampler, float2(gradientSample, 0.)) * step(cutoff, sample);

    return paletteColor * seaTargetSample.b;
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}