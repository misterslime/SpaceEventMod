#define TAU 6.28318530718
#define PI 3.14159265359
#define PIOVER2 1.57079632679

#define GRAYSCALE float3(0.2126729, 0.7151522, 0.0721750)

float cro( in float2 a, in float2 b ) 
{ 
    return a.x*b.y - a.y*b.x; 
}

float2 rotate(in float2 uv, float angle) 
{
    float2x2 rotate2d = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
    return mul(rotate2d, (uv - 0.5) + 0.5);
}

float2 angleVector(float angle)
{
    return float2(cos(angle), sin(angle));
}

float easeOutSine(float x)
{
    return 1. - cos((x * PI) / 2.);
}