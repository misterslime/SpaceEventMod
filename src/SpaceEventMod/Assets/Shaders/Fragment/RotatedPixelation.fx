#include "../Math.fxh"
#include "../Pixelate.fxh"

sampler uImage0 : register(s0);

float angle;
float pixelSize;
float2 resolution;

float4 frag(float2 uv : TEXCOORD0) : COLOR0 
{
    // Rotate uv coordinates
    float2 coords = rotate(uv, angle);
    
    // Quantize the uv coordinates
    float2 pixelCoords = floor((coords * resolution) / pixelSize) * pixelSize + (pixelSize / 2.0);

    // Rotate back to sample the original texture orientation
    pixelCoords = rotate(pixelCoords / resolution, -angle);

    return tex2D(uImage0, pixelCoords);
}

technique Technique1 {
    pass RotatedPixelatedPass {
        PixelShader = compile ps_3_0 frag();
    }
};