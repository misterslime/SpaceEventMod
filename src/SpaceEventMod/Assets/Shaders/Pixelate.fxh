float2 pixelate(float2 coord, float2 pixelSize) 
{
    return floor(coord / pixelSize) * pixelSize;
}