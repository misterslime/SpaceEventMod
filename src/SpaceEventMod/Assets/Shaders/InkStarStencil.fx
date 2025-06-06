sampler star : register(s0);

texture ink; //controls the density of clouds across the whole seamap
sampler inkSampler = sampler_state
{
    Texture = (ink);
};

float2 screenSize;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float2 screenCoords = screenPosition / screenSize;
    
    return sampleColor *  tex2D(inkSampler, screenCoords).a;
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}