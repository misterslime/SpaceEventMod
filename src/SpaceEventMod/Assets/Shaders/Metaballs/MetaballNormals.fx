sampler sdfTarget : register(s0);

float circ(float r, float pos)
{
    return sqrt((r * r) - (pos * pos));
}

float3 unpack(float4 color) 
{
    float2 gradient = 2.0 * (color.rg - 0.5);
    float distance = (color.a > 0.) ? -color.b : (1.0 / color.b) - 1.0;
    return float3(distance, gradient);
}

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float4 sample = tex2D(sdfTarget, uv);

    float3 total = unpack(sample);

    // laziest fix in history
    float2 d = float2(abs(clamp(1.0 + total.x, 0.0001, 1.0)), 0.0);

    d.y = abs(circ(1.0, d.x));

    float2 g = total.yz * d.x;
    
	// coloring
    float b = 0.5 * d.y;
    float3 col = float3(0.5+0.5*g.x, 0.5+0.5*g.y, 0.5 + b);

    return float4(col.x, col.y, col.z, 1.0-step(0.0,total.x));
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}