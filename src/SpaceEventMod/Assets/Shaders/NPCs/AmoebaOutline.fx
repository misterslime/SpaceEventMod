sampler metaballs : register(s0);

float4 PixelShaderFunction(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0 
{
    float4 sample = tex2D(metaballs, coords);
    return color * sample.a;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}