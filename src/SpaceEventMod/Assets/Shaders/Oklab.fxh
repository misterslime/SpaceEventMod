float3 oklabMix( float3 colA, float3 colB, float h )
{
    // https://bottosson.github.io/posts/oklab
    const float3x3 kCONEtoLMS = float3x3(                
         0.4121656120,  0.2118591070,  0.0883097947,
         0.5362752080,  0.6807189584,  0.2818474174,
         0.0514575653,  0.1074065790,  0.6302613616);

    const float3x3 kLMStoCONE = float3x3(
         4.0767245293, -1.2681437731, -0.0041119885,
        -3.3072168827,  2.6093323231, -0.7034763098,
         0.2307590544, -0.3411344290,  1.7068625689);
                    
    // rgb to cone (arg of pow can't be negative)
    float3 lmsA = pow( mul(kCONEtoLMS, colA), 0.333333 );
    float3 lmsB = pow( mul(kCONEtoLMS, colB), 0.333333 );

    // lerp
    float3 lms = lerp( lmsA, lmsB, h );

    // gain in the middle (no oaklab anymore, but looks better?)
    //lms *= 1.0+0.2*h*(1.0-h);

    // cone to rgb
    return mul(kLMStoCONE, (lms*lms*lms));
}