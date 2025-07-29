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
};

float2 screenSize;
float2 screenWorldPosition;
float globalTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float2 screenWorldCoords = (screenPosition + screenWorldPosition) / screenSize;

    float2 noiseCoords = float2(globalTime * 0.007, globalTime * -0.002);
    float sample = (tex2D(noiseSampler, screenWorldCoords + noiseCoords).r + tex2D(seaTarget, coords).r) * 0.5f;
    
    float mask = step(0.5, sample);
	
    return float4(mask, mask, mask, mask);
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}