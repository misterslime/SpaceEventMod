#include "../Blur.fxh"

sampler uImage0 : register(s0);
float blurRadius;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0 
{
    float4 baseColor = tex2D(uImage0, uv);
    float blurColor = blur(uImage0, uv, blurRadius).r;
    
    return float4(blurColor, blurColor, blurColor, blurColor) * (1 - round(baseColor.a));
}

technique Technique1 {
    pass Pass0 {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};