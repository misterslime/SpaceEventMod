struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 Coords : TEXCOORD0;
};

sampler uImage0 : register(s0);

texture TileTarget;
sampler TileTargetSampler = sampler_state
{
    Texture = (TileTarget);
};

texture TileTarget2;
sampler TileTarget2Sampler = sampler_state
{
    Texture = (TileTarget2);
};

uniform float2 TileTargetPosition;
uniform float2 TileTargetSize;

uniform float2 TileTarget2Position;
uniform float2 TileTarget2Size;

uniform float2 ScreenPosition;
uniform float2 ScreenZoomRatio;

float4 PixelShaderFunction(in PSInput input) : COLOR0
{
    float4 texSample = tex2D(uImage0, input.Coords);
    
    // Convert to world pixel coordinates.
    // Multiply screen pixel position by zoom ratio so it actually zooms in.
    float2 screenWorldCoords = ScreenPosition + input.Position.xy;
    
    // Map world coordinates to uv coordinates on the tile targets.
    float2 tileTargetCoords = (screenWorldCoords - TileTargetPosition) / TileTargetSize;
    float2 tileTarget2Coords = (screenWorldCoords - TileTarget2Position) / TileTarget2Size;
    
    // Multiply samples together
    float4 tileTargetSample = tex2D(TileTarget2Sampler, tileTarget2Coords) + tex2D(TileTargetSampler, tileTargetCoords);
    
    return tex2D(uImage0, input.Coords) * saturate(tileTargetSample.a) * input.Color;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}