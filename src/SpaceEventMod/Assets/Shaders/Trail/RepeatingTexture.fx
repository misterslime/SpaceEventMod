matrix transformMatrix;

int spriteRotation;
float repeats;

texture sampleTexture;
sampler sampleTextureSampler = sampler_state
{
    Texture = (sampleTexture);
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = wrap;
    AddressV = wrap;
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

const float2 rotationTable[8] = {
    float2(1, 0),
    float2(0, 1),

    float2(0, -1),
    float2(1, 0),

    float2(-1, 0),
    float2(0, -1),

    float2(0, 1),
    float2(-1, 0)
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output;
	
    output.Position = mul(input.Position, transformMatrix);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    int index = spriteRotation % 4;

    float2 uv = input.TextureCoordinates;

    uv.x = uv.x * repeats;

    float2 rotX = rotationTable[index * 2];
    float2 rotY = rotationTable[index * 2 + 1];

    uv = float2(
        rotX.x * uv.x + rotX.y * uv.y, 
        rotY.x * uv.x + rotY.y * uv.y
    );

    return input.Color * tex2D(sampleTextureSampler, uv);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}