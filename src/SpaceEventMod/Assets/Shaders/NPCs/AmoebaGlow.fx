sampler sdfSampler : register(s0);

float dropoff;

float4 PixelShaderFunction(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0 
{
    float4 sample = tex2D(sdfSampler, coords);

    float drop = pow(sample.b, dropoff);

    return drop * color * (1.0 - step(0.5, sample.a));
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}