#include "../EdgeDetection.fxh"
#include "../Pixelate.fxh"
#include "../Math.fxh"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float minAlpha = uColor.r;
    float lightAngle = uColor.g;

    float2 pixelSize = 2. / uScreenResolution;

    float2 pixelation = pixelate(uv, pixelSize);

    float4 sample = tex2D(uImage1, pixelation);

    float2 polar = float2(lightAngle, (sample.b - 0.5) * 2.0);
    polar.x += (atan2(sample.r - 0.5, sample.g - 0.5) + PI) / TAU;

    float alpha = lerp(1.0, minAlpha, polar.y);

    float4 bodyColor = tex2D(uImage2, polar);
    bodyColor.a = alpha;
    bodyColor *= sample.a;

    float edge = edgeDetection(uImage1, pixelation, pixelSize) / 15.;

    float4 edgeColor = tex2D(uImage3, polar);

    float4 final = (edge > 0.) ? edgeColor : bodyColor;

    float2 displace = float2(0., 0.);

    displace.y += (sample.g - 0.5) / (sample.b - 0.5);
    displace.x += (sample.r - 0.5) / (sample.b - 0.5);

    displace *= sample.a * 0.15;

    float4 screenColor = tex2D(uImage0, displace + uv);

    return (final * final.a) + (screenColor * (1.0 - final.a));
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}