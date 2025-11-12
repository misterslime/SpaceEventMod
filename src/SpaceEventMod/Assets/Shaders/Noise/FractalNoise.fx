const float2x2 mtx = float2x2 ( float2(0.80, -0.60), float2(0.60, 0.80) );

uniform float time;
uniform float2 screenSize;
uniform float zoom;

uniform float2 displacementA;
uniform float2 displacementB;

uniform float4 backgroundColor;
uniform float4 lowColor;
uniform float4 middleColor;
uniform float4 highColor;

uniform float gradientPixelation;
uniform float backgroundThreshold;
uniform float lowColorThreshold;
uniform float midColorThreshold;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float rand(float2 n) 
{ 
    return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
}

float noise(float2 p)
{
    float2 ip = floor(p);
    float2 u = frac(p);
    u = u * u * (3.0 - 2.0 * u);

    float res = lerp(
        lerp(rand(ip), rand(ip + float2(1.0, 0.0)), u.x),
        lerp(rand(ip + float2(0.0, 1.0)), rand(ip + float2(1.0, 1.0)), u.x), u.y);
        
    return res * res;
}

float fbm( float2 p )
{
    float f = 0.0;
    
    f += 0.500000 * noise( p + time ); p = mul(mtx,p) * 2.02;
    f += 0.031250 * noise( p ); p = mul(mtx,p) * 2.01;
    f += 0.250000 * noise( p ); p = mul(mtx,p) * 2.03;
    f += 0.125000 * noise( p ); p = mul(mtx,p) * 2.01;
    f += 0.062500 * noise( p ); p = mul(mtx,p) * 2.04;
    f += 0.015625 * noise( p + sin(time) );

    return f / 0.96875;
}

float pattern( in float2 p )
{
	return fbm( p + fbm( p + fbm( p ) ) );
}

float3 colormap(float x, float2 uv) 
{
    if (x < backgroundThreshold) { 
        return backgroundColor.rgb;
    }
    else if (x < lowColorThreshold) { 
        return lerp(
			backgroundColor.rgb,
			lowColor.rgb,
			round((x - backgroundThreshold) / (lowColorThreshold - backgroundThreshold) / gradientPixelation) * gradientPixelation
		);
    }
    else if (x < midColorThreshold) { 
        return 
			lerp(
				lowColor.rgb,
				middleColor.rgb,
				round((x - lowColorThreshold) / (midColorThreshold - lowColorThreshold) / gradientPixelation) * gradientPixelation
			);
    }
	else
		return
			lerp(
				middleColor.rgb,
				highColor.rgb,
				round((x - midColorThreshold) / (1.0 - midColorThreshold) / gradientPixelation) * gradientPixelation
			);
}


float4 PixelShaderFunction(float2 coords : TEXCOORD0, float2 screenPosition : SV_POSITION, float4 sampleColor : COLOR0) : COLOR0
{
    float2 worldPixelPosition = screenPos + coords * worldViewDimensions;

    float2 the = displacementA * time + worldPixelPosition;
    float2 the2 = displacementB * time + worldPixelPosition;

    float shade = (pattern(the * zoom) + pattern(the2 * zoom)) * 0.5f;

    float3 color = colormap(shade, worldPixelPosition);

    return float4(color, 1.0);
}

technique Technique0
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}