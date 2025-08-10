sampler seaTarget : register(s0);

texture noise;
sampler noiseSampler = sampler_state
{
    Texture = (noise);
};

texture palette;
sampler paletteSampler = sampler_state
{
    Texture = (palette);
    Filter = MIN_MAG_MIP_POINT;
    AddressU = clamp;
    AddressV = clamp;
};

float2 screenSize;
float2 screenWorldPosition;
float globalTime;
float parallax;

float2 Pixelate(float2 coord, float2 pixelSize) 
{
    return floor(coord / pixelSize) * pixelSize;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float2 pixelSize = 2. / screenSize;

    //pixelate
    float2 screenWorldCoords = Pixelate((screenPosition + screenWorldPosition * parallax) / screenSize, pixelSize);
    float2 noiseCoords = Pixelate(screenWorldCoords + float2(globalTime * 0.01, globalTime * 0.005), pixelSize);

    coords = Pixelate(coords, pixelSize);

    // noise sampling
    float sample = (tex2D(noiseSampler, noiseCoords).g + tex2D(seaTarget, coords).g) * 0.5;
    float4 paletteColor = step(0.5, sample) * tex2D(paletteSampler, float2(sample, 0.));

    // edge detection
    float edge = 0.;
    edge += tex2D(seaTarget, Pixelate(coords + float2(0., 1.) / screenSize, pixelSize)).b;
    edge += tex2D(seaTarget, Pixelate(coords + float2(1., 0.) / screenSize, pixelSize)).b;
    edge += tex2D(seaTarget, Pixelate(coords + float2(0., -1.) / screenSize, pixelSize)).b;
    edge += tex2D(seaTarget, Pixelate(coords + float2(-1., 0.) / screenSize, pixelSize)).b;
    edge = clamp(edge, 0., 1.) - tex2D(seaTarget, coords).b;

    // the outline color is the thing u feed into spriteBatch
    return sampleColor * edge + paletteColor;
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}