#include "../Oklab.fxh"
#include "../Blur.fxh"

sampler uImage0 : register(s0);
sampler distort : register(s1);

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
float4 uShaderSpecificData;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 fogColor; // float4(0., 0.25, 1.0, 0.3);
float fogStart; // 0.05;
float fogEnd; // 0.65;
float distortIntensity; // 0.05;
float distortNoiseScale; // 1.0;
float timeScale; // 0.05;
float blurMulti; // 1.0;

float4 PixelShaderFunction(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 uv = coords;
    float toCenter = length(float2(0.5, 0.5) - uv);

    // Get fog value
    float fogValue = toCenter < fogEnd ? smoothstep(fogStart, fogEnd, toCenter) : 1.0;
    fogValue *= fogColor.a;
    
    // Sample distortion noise
    float2 distortionUv = uScreenPosition + uv * uScreenResolution; 
    distortionUv *= distortNoiseScale;

    float2 distortionNoise = tex2D(distort, distortionUv + float2(uTime, uTime) * timeScale).rg;
    distortionNoise *= tex2D(distort, distortionUv + float2(-uTime, uTime) * timeScale).rg;
    
    // Displace uv coordinates by sampled distortion vector
    uv += distortionNoise * distortIntensity * fogValue;
    
    // Blur sampled color because it makes it look underwater n stuff
    float4 baseColor = float4(blur(uImage0, uv, fogValue * blurMulti), 1.0);
    
    // Mix fog and base colors in oklab color space (looks better)
    float3 final = oklabLerp(baseColor * (1. - fogValue), fogColor * fogValue, fogValue).rgb;

    return float4(final, baseColor.a);
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}