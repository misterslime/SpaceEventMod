float quantize(float value, float steps) 
{
    return floor(value * steps) / steps;
}

float2 pixelate(float2 coord, float2 pixelSize) 
{
    return floor(coord / pixelSize) * pixelSize;
}

float3 quantizeColor(float3 color, float levels) 
{
    return floor(color * levels) / levels;
}