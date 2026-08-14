static const float TAU = 6.28318530718;
static const float PI = 3.14159265359;
static const float PIOVER2 = 1.57079632679;

float cro( in float2 a, in float2 b ) 
{ 
    return a.x*b.y - a.y*b.x; 
}

float2 rotate(in float2 uv, float angle) 
{
    float2x2 rotate2d = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
    return mul(rotate2d, (uv - 0.5) + 0.5);
}