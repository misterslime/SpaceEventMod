sampler nucleus : register(s0);

texture flowmap;
sampler flowmapSampler = sampler_state
{
    Texture = (flowmap);
    Filter = MIN_MAG_MIP_LINEAR;
};

float scale;
float strength;
float2 flowDisplacement;

float2 screenPosition;
float2 worldViewDimensions;

float4 PixelShaderFunction(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0 
{
    float2 worldPixelPosition = screenPosition + coords * worldViewDimensions * scale;

    float4 sample = tex2D(flowmapSampler, worldPixelPosition + flowDisplacement);

    float2 displace = sample.rg * 2. - 1.;

    return tex2D(nucleus, coords + displace * strength);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}