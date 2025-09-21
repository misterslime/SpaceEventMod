matrix transformMatrix;

bool flipVertically = false;
bool flipHorizontally = false;

// (current horizontal frame, current vertical frame, horizontal frames, vertical frames)
float4 frame = float4(0., 0., 1., 1.);

texture sampleTexture;
sampler sampleTextureSampler = sampler_state
{
    Texture = (sampleTexture);
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output;
	
    output.Position = mul(input.Position, transformMatrix);
    output.Color = input.Color;
    float2 coords = input.TextureCoordinates;
	
    float2 frameScale = float2(1. / frame.z, 1. / frame.w);

    coords.x = coords.x * frameScale.x;
    coords.y = coords.y * frameScale.y;

    if (flipVertically)
        coords.y = frameScale.x - coords.y;
    if (flipHorizontally)
        coords.x = frameScale.y - coords.x;

    coords.x = coords.x + frame.x * frameScale.x;
    coords.y = coords.y + frame.y * frameScale.y;

    output.TextureCoordinates = coords;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 textureColor = tex2D(sampleTextureSampler, input.TextureCoordinates);
    return textureColor * input.Color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}