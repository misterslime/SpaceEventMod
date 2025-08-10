sampler seaTarget : register(s0);

texture sea;
sampler seaSampler = sampler_state
{
    Texture = (sea);
};

float minimumAlpha;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float4 seaTargetSample = tex2D(seaTarget, coords);

    return seaTargetSample * clamp(tex2D(seaSampler, coords).r, minimumAlpha, 1.);
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}