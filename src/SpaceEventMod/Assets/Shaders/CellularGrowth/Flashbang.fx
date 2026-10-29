#include "../SignedDistanceFunctions.fxh"

// ignore these theyre for convenience
#define position xy
#define strength z
#define radius w
#define innerFlash innerTotal.x
#define outerFlash outerTotal.x
#define shadowFlash shadowTotal.x

sampler uImage0 : register(s0);
sampler wallsTarget : register(s1);
sampler foregroundTarget : register(s2);

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

uniform float4 flashData[8];
uniform int flashCount;

float4 flashColor;
float4 shadowColor;
//float2 flashPosition;
float2 screenTargetRatio;
//float flashStrength;
//float flashRadius;

struct PSInput
{
    float4 Color : COLOR0;
    float2 TexCoords : TEXCOORD0;
    float2 Position : SV_POSITION;
};

struct PSOutput
{
    float4 Color : COLOR0;
};

float4 alphablend(float4 src, float4 dest)
{
    return src.a * src + (1.0 - src.a) * dest;
}

float2 moveAway(float2 sample, float2 awayFrom, float mul)
{
    float2 p = sample - awayFrom;
    p *= mul;
    return p + awayFrom;
}

PSOutput PixelShaderFunction(PSInput input)
{
    PSOutput output;
    
    float threshhold = 0.000000001;
    float2 uv = input.TexCoords;
    uv /= screenTargetRatio;
    
    // Get flash sdf values
    float3 innerTotal = float3(99999.0, 0., 0.);
    float3 outerTotal = float3(99999.0, 0., 0.);
    float3 shadowTotal = float3(99999.0, 0., 0.);
    float shadowSample = 0.0;
    float flashStrength = 0.67;
    float4 godrayColor = 0.0;

    for (int i = 0; i < flashCount; i++)
    {
        float3 inner = sdgCircle(uv, flashData[i].position, 0.8 * flashData[i].radius);
        float3 outer = sdgCircle(uv, flashData[i].position, 0.3 * flashData[i].radius) * 0.4;
        float3 shadow = sdgCircle(uv, flashData[i].position, 2.8 * flashData[i].radius);
        
        innerTotal = smin(innerTotal, inner, 0.2);
        outerTotal = smin(outerTotal, outer, 0.2);
        shadowTotal = smin(shadowTotal, shadow, 0.2);
        
        // Add shadow values for each light source.
        shadowSample += step(threshhold, dot(tex2D(foregroundTarget, moveAway(uv, flashData[i].position, 0.96)).rgb, GRAYSCALE)) * (1. - outer.x);
        
        // Add subtle spike/god ray type things for artistic effect.
        float godrayValue = step(0.95, dot(normalize(outer.yz), normalize(angleVector(flashData[i].strength + uTime * -2.6))));
        godrayValue += step(0.97, dot(normalize(outer.yz), normalize(angleVector(flashData[i].strength + uTime * 1.9))));
        godrayValue += step(0.97, dot(normalize(outer.yz), normalize(angleVector(flashData[i].strength + uTime * -0.74))));
        godrayValue += step(0.97, dot(normalize(outer.yz), normalize(angleVector(flashData[i].strength + uTime * -1.33))));
        godrayValue += step(0.97, dot(normalize(outer.yz), normalize(angleVector(flashData[i].strength + uTime))));
        
        godrayColor += float4(1.0, 0.8, 0.9, 0.0) * step(0.97, godrayValue) * 0.1 / clamp(length(uv - flashData[i].position) * 4, 0.5, 20.0);
    }
    
    flashStrength = saturate(flashStrength);
    shadowSample = saturate(shadowSample / flashCount);
    
    innerTotal.x = easeOutSine(saturate(-innerTotal.x));
    innerTotal.yz *= -1;
    
    shadowTotal.x = easeOutSine(saturate(-shadowTotal.x));
    shadowTotal.yz *= -1;
    
    outerTotal.x = easeOutSine(saturate(outerTotal.x));
    
    // Sample shadow color.
    //float shadow = step(threshhold, dot(tex2D(foregroundTarget, moveAway(uv, outerTotal.yz, 0.96)).rgb, GRAYSCALE)) * (1. - outerFlash);
    float3 shadowColor2 = shadowColor.rgb;
    
    // Acquire color of the wall target and use its lightness value to determine whether it should have an alpha of 0 or not.
    float4 wallColor = tex2D(wallsTarget, uv);
    float wallAlpha = step(threshhold, dot(wallColor.rgb, GRAYSCALE));
    
    // Subtle color correction/darkening to bring attention to the flash effect.
    wallColor.rgb -= shadowColor2 * shadowFlash;
    
    // Mix inner white flash with outer colored flash values.
    wallColor += lerp(flashColor * 0.7, float4(0.0, 0.0, 0.0, 0.0), outerFlash) * (1. - shadowSample);
    wallColor += lerp(wallColor, float4(1.0, 1.0, 1.0, 1.0), innerFlash * (1. - shadowSample));
    wallColor.rgb -= shadowColor2 * shadowFlash * 0.12;
    wallColor.a = 1.0;
    
    // Stencil
    wallColor *= wallAlpha;
    wallColor = lerp(tex2D(uImage0, input.TexCoords), wallColor, flashStrength);
    
    // Acquire color of the everything else target and use its lightness value to determine whether it should have an alpha of 0 or not.
    float4 foregroundColor = tex2D(foregroundTarget, uv);
    float foregroundAlpha = step(threshhold, dot(foregroundColor.rgb, GRAYSCALE));
    
    // Apply similar steps as to the wall color.
    foregroundColor.rgb -= shadowColor2 * shadowFlash;
    foregroundColor += lerp(flashColor, float4(0.0, 0.0, 0.0, 0.0), outerFlash) * 0.4;
    foregroundColor += lerp(foregroundColor, float4(1.0, 1.0, 1.0, 1.0), innerFlash) * foregroundColor.a;
    foregroundColor.a = 1.0;
    foregroundColor *= foregroundAlpha;
    foregroundColor = lerp(tex2D(uImage0, input.TexCoords), foregroundColor, flashStrength);
    
    // get bg color
    float stencil = step(threshhold, (foregroundAlpha + wallAlpha)) * (innerFlash + (1. - outerFlash) + shadowFlash);
    float4 backgroundColor = tex2D(uImage0, input.TexCoords);
    backgroundColor.rgb -= shadowColor2 * shadowFlash * flashStrength;
    
    float4 final = lerp(backgroundColor, alphablend(foregroundColor, wallColor), saturate(stencil));
    
    // Add subtle spike/god ray type things for artistic effect.
    final += godrayColor * pow((1. - outerFlash), 2.2) * flashStrength;
    
    output.Color = final;
    
    return output;
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}