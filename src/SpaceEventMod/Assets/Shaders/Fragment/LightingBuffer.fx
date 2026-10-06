sampler uImage0 : register(s0);

texture LightingBuffer;
sampler2D LightingBufferSampler = sampler_state
{
    Texture = (LightingBuffer);
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

struct PixelShaderInput
{
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    float4 light = tex2D(LightingBufferSampler, input.TextureCoordinates);
    float4 color = tex2D(uImage0, input.TextureCoordinates);
    
    return color * light * input.Color;
}

technique Technique1
{
    pass LightedTargetPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}