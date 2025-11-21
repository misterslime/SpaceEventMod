struct PSInput
{
    float4 Color : COLOR0;
    float2 Coords : TEXCOORD0;
};

struct PSOutput
{
    float4 Color : COLOR0;
};

const float3 VIEW_DIRECTION = float3(0., 0., 1.);

sampler normals : register(s0);

texture bodyTarget;
sampler bodySampler = sampler_state
{
    Texture = (bodyTarget);
    Filter = MIN_MAG_MIP_POINT;
};


texture outlineTarget;
sampler outlineSampler = sampler_state
{
    Texture = (outlineTarget);
    Filter = MIN_MAG_MIP_POINT;
};

float3 incomingLight;

float pixelation;
float shininess;
float shadowThreshold;
float4 shadowColor;

PSOutput PixelShaderFunction(PSInput input)
{
    PSOutput output;

    float4 sample = tex2D(normals, input.Coords);
    float4 surfaceColor = tex2D(bodySampler, input.Coords);

    float3 normal = (sample.rgb - 0.5) * 2.;
    float3 half = normalize(incomingLight + VIEW_DIRECTION);

    float specular = round(pow(dot(normal, half), shininess) / pixelation) * pixelation;
    float4 shadow = shadowColor * (1. - step(shadowThreshold, dot(normal, incomingLight)));

    float4 outputColor = (shadow * shadow.a) + ((surfaceColor + specular) * (1.0 - shadow.a));
    outputColor *= step(0.5, sample.a);

    float4 outlineColor = tex2D(outlineSampler, input.Coords) * (1. - step(0.5, sample.a));

    output.Color = outputColor + outlineColor;

    return output;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}