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
    AddressU = clamp; 
    AddressV = clamp;
};

float2 screenSize;
float2 screenWorldPosition;
float globalTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float2 screenWorldCoords = (screenPosition + screenWorldPosition) / screenSize;

    float2 noiseCoords = float2(globalTime * 0.01, globalTime * 0.003);
    float sample = (tex2D(noiseSampler, screenWorldCoords + noiseCoords).r + tex2D(seaTarget, coords).r) * 0.5;
    
    float4 paletteColor = step(0.5, sample) * tex2D(paletteSampler, float2(sample, 0.));
	
    return paletteColor;
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}