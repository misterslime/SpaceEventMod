struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 Coords : TEXCOORD0;
};

sampler WallTexture : register(s0);

texture OverlayTexture;
sampler OverlaySampler = sampler_state
{
    Texture = (OverlayTexture);
    Filter = MIN_MAG_MIP_POINT;
    AddressU = wrap;
    AddressV = wrap;
};

uniform float4 OutlineColor;
uniform float2 ScreenPosition;
uniform float2 PixelSize;
uniform float Parallax;

float4 PixelShaderFunction(in PSInput input) : COLOR0
{
    // Get position of pixel in world
    
    float2 screenWorldCoords = (input.Position.xy + (ScreenPosition * Parallax)) * PixelSize;

    //float2 worldPixelPosition = ScreenPosition + input.Position.xy;
    
    
    float4 texSample = tex2D(WallTexture, input.Coords);
    
    // Red in the base wall texture defines that this color is the outline color
    float4 outline = texSample.r * OutlineColor;
    
    // Green in the base wall texture defines that this color is the overlay color
    float4 overlay = texSample.g * tex2D(OverlaySampler, screenWorldCoords);
    
    return (outline + overlay) * texSample.a;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}