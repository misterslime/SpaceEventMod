#include "../Blur.fxh"

sampler uImage0 : register(s0);

texture noise;
sampler noiseSampler = sampler_state
{
    Texture = (noise);
};

texture sea;
sampler seaSampler = sampler_state
{
    Texture = (sea);
};


float2 pixelSize;
float uTime;
float uScale;
float factor;
int quantization;

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float4 baseColor = tex2D(uImage0, uv);

    // Get normal vector from blurred target
    float top = tex2D(uImage0, uv - float2(0., pixelSize.y)).r;
    float bottom = tex2D(uImage0, uv + float2(0., pixelSize.y)).r;
    float left = tex2D(uImage0, uv - float2(pixelSize.x, 0.)).r;
    float right = tex2D(uImage0, uv + float2(pixelSize.x, 0.)).r;

    float3 normal = normalize(float3(2*(right-left), 2*(bottom-top), -4));

    // Use polar coordinates for texture samples
    float2 polar = float2(atan2(normal.x, normal.y)/6.2832, 0.)+.5;
    polar.y = baseColor.r * uScale + uTime;

    float4 rippleColor = tex2D(noiseSampler, polar);


    // Quantize the ripple color so it looks more like pixel art
    rippleColor = round(rippleColor * quantization) / quantization;

    // Non-linear color ramp
    rippleColor = pow(rippleColor, factor);

    float4 final = rippleColor * baseColor.r * (1 - round(baseColor.a)) * color;

    // Use the sea target to ensure the tile ripples arent present outside the sea
    return final * tex2D(seaSampler, uv).b;
}

technique Technique1 {
    pass Pass0 {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};