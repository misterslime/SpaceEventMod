static const float TAU = 6.28318530718;
static const float PI = 3.14159265359;
static const float PIOVER2 = 1.57079632679;

float2 rotateUV(float2 uv, float rotation)
{
    float cosAngle = cos(rotation);
    float sinAngle = sin(rotation);

    float2 point = uv - float2(0.5);

    return float2(cosAngle * point.x + sinAngle * point.y + 0.5, cosAngle * point.y - sinAngle * point.x + 0.5);
}

float2 rotateUV(float2 uv, float rotation, vec2 mid)
{
    float cosAngle = cos(rotation);
    float sinAngle = sin(rotation);

    float2 point = uv - mid;

    return float2(cosAngle * point.x + sinAngle * point.y + mid.x, cosAngle * point.y - sinAngle * point.x + mid.y);
}

float2 rotateUV(float2 uv, float rotation, float mid)
{
    float cosAngle = cos(rotation);
    float sinAngle = sin(rotation);

    float2 point = uv - float2(mid);

    return float2(cosAngle * point.x + sinAngle * point.y + mid, cosAngle * point.y - sinAngle * point.x + mid);
}