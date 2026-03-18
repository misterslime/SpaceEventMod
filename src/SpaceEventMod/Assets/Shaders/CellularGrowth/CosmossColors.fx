sampler glow : register(s0);

texture colorMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    Filter = MIN_MAG_MIP_POINT;
    AddressU = clamp; 
    AddressV = clamp;
};

texture noiseTexture;
sampler noiseSampler = sampler_state
{
    Texture = (noiseTexture);
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = clamp; 
    AddressV = clamp;
};

uniform float sineAmp; // 0.02
uniform float sineStrength; // 0.5
uniform float verticalSineAmp; // 0.05
uniform float verticalSineStrength; // 0.5
uniform float noiseScale;
uniform float noiseStrength;
uniform float mixQuantization;
uniform float uTime;
uniform float2 resolution;
uniform float2 tilePos;
uniform float4 sourceRect;

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    // Add 1/6 because the glow value shifts it 1 shade darker than it shoudl be
    float glowValue = tex2D(glow, uv).r + (1. / 6.);
    
    // Get alpha value to cut out pixels that aren't part of the glow texture
    float glowMask = tex2D(glow, uv).a;

    // Get world pixel position
    float2 frameUv = (uv * resolution - sourceRect.xy) / sourceRect.zw;
    float2 worldPixelPosition = tilePos + frameUv;

    // Sample noise to shift sine value a lil
    float noiseValue = tex2D(noiseSampler, worldPixelPosition * noiseScale - uTime / 15.).r;
    
    // Use sine waves to mix between red and orange colors
    float sineY = sin(noiseValue + worldPixelPosition.y * verticalSineAmp - uTime * 0.2);
    sineY += sin(noiseValue + worldPixelPosition.y * verticalSineAmp + uTime * 0.4);
    float mixValue = sin(noiseValue + worldPixelPosition.x * sineAmp + sineY * verticalSineStrength + uTime * 1.) * sineStrength;
    
    // Mix noise and sine values
    mixValue += noiseValue * noiseStrength;
    
    // Quantize mix value
    mixValue = floor(mixValue * mixQuantization) / mixQuantization;
    
    // Sample and mix red and orange palettes
    float4 red = tex2D(colorSampler, float2(glowValue, 0.9999));
	float4 orange = tex2D(colorSampler, float2(glowValue, 0.));
    
    return lerp(red, orange, mixValue) * glowMask;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}