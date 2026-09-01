sampler glow : register(s0);

texture colorMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    Filter = MIN_MAG_MIP_POINT;
    AddressU = clamp; 
    AddressV = clamp;
};

uniform float sineAmp; // 0.02
uniform float sineStrength; // 0.5
uniform float verticalSineAmp; // 0.05
uniform float verticalSineStrength; // 0.5
uniform float uTime;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    // Add 1/6 because the glow value shifts it 1 shade darker than it shoudl be
    float glowValue = tex2D(glow, uv).r + (1. / 6.);
    
    // Get alpha value to cut out pixels that aren't part of the glow texture
    float glowMask = tex2D(glow, uv).a;

    // Get world pixel position
    float2 worldPixelPosition = screenPos + uv * worldViewDimensions;

    // Use sine waves to mix between red and orange colors
    float sineY = sin(worldPixelPosition.y * verticalSineAmp - uTime * 0.2);
    sineY += sin(worldPixelPosition.y * verticalSineAmp + uTime * 0.4);
    float mixValue = sin(worldPixelPosition.x * sineAmp + sineY * verticalSineStrength + uTime * 1.) * sineStrength;
    
    // Sample and mix red and orange palettes
    float4 red = tex2D(colorSampler, float2(glowValue, 0.9999));
	float4 orange = tex2D(colorSampler, float2(glowValue, 0.));
    
    return color * lerp(red, orange, step(0.5,mixValue)) * glowMask;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}