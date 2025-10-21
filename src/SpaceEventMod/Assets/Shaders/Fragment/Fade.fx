sampler uImage0 : register(s0);
float fade;

float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    float4 s = tex2D(uImage0, uv);
    return float4(s.rgb, s.a * fade);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 frag();
    }
};