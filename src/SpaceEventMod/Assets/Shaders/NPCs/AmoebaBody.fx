sampler normalSampler : register(s0);

texture noiseTarget;
sampler noiseSampler = sampler_state
{
    Texture = (noiseTarget);
    Filter = MIN_MAG_MIP_POINT;
};

float2 pixelSize;
float displacement;
float minAlpha;

float4 PixelShaderFunction(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0 
{
    float4 sample = tex2D(normalSampler, coords);

    float2 displace = float2(0., 0.);

    displace.y += (sample.g - 0.5) / (sample.b - 0.5);
    displace.x += (sample.r - 0.5) / (sample.b - 0.5);

    displace *= sample.a * pixelSize * displacement;

    float4 noiseSample = tex2D(noiseSampler, displace + coords);
	noiseSample.a = lerp(1.0, minAlpha, (sample.b - 0.5) * 2.0);

    return (sample.a > 0.) ? noiseSample : float4(0.0, 0.0, 0.0, 0.0);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}