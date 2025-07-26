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
float globalTime;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 noiseCoords = float2(coords.x + globalTime * 0.007, coords.y + globalTime * -0.002);
    float sample = (tex2D(noiseSampler, noiseCoords).r * 0.45) + (tex2D(seaTarget, coords).r * 0.65);
    
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