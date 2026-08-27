#define VIEW_DIRECTION float3(0., 0., 1.)

struct PSInput
{
    float4 Color : COLOR0;
    float2 Coords : TEXCOORD0;
};

texture noiseTexture;
sampler noiseSampler = sampler_state
{
    Texture = (noiseTexture);
    Filter = MIN_MAG_MIP_LINEAR;
};

texture normalTexture;
sampler normalSampler = sampler_state
{
    Texture = (normalTexture);
    Filter = MIN_MAG_MIP_LINEAR;
};

uniform float3 aData[64];
uniform float2 bData[64];
uniform int lineCount;
uniform float smoothness;
uniform float uTime;
uniform float zoom;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float4 sdgSegment( float3 p, float3 a, float3 b, float r )
{
    float3 ba = b-a;
    float3 pa = p-a;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    float3  q = pa-h*ba;
    float d = length(q);
    return float4(d-r,q/d);    
}

float4 smin3D( in float4 a, in float4 b, in float k )
{
    k *= 4.0;
    float h = max(k-abs(a.x-b.x),0.0)/(2.0*k);
    return float4(min(a.x,b.x)-h*h*k,
                lerp(a.yzw,b.yzw,(a.x<b.x)?h:1.0-h));
}

float4 sampleSegments( float3 p ) 
{
    float4 total = float4(99999.0, 0., 0., 0.);

    for (int i = 0; i < lineCount; i++) 
    {
        float3 pointA = float3(aData[i].x, aData[i].y, 0.0) / zoom;
        float3 pointB = float3(bData[i].x, bData[i].y, 0.0) / zoom;

        float4 dist = sdgSegment(p, pointA, pointB, aData[i].z);

        total = smin3D(total, dist, smoothness);
    }

    return total;
}

float specularBRDF(float3 normal, float3 lightVector, float shininess) 
{
	float3 reflection = -lightVector - 2 * dot(-lightVector, normal) * normal;
	return pow(dot(VIEW_DIRECTION, reflection), shininess);
}

float3 fresnelColor(float3 n)
{
	// Tri-linear fresnel color mapping
    float3 colXZ = float3(1.0, 1.0, 0.2);
    float3 colYZ = float3(0.0, 1.0, 0.0);
    float3 colXY = float3(0.0, 0.0, 0.0);
      
    n = abs(n);
        
    //n *= pow(n, float3(2));
    n /= n.x+n.y+n.z;
        
    return colYZ*n.x + colXZ*n.y + colXY*n.z;
}

float4 PixelShaderFunction(PSInput input) : COLOR0 
{
    float2 worldPixelPosition = screenPos + input.Coords * worldViewDimensions;

    worldPixelPosition /= zoom;

    // Raymarching
    const float tmax = 5.0;
    float3 ro = float3(worldPixelPosition.x, worldPixelPosition.y, 3.);

    float4 total = sampleSegments(float3(worldPixelPosition.x, worldPixelPosition.y, 0.0));
    if (total.x >= 0.0)
    {
    	return float4(0.0, 0.0, 0.0, 0.0);
    }
    
    total = float4(99999.0, 0., 0., 0.);
    float t = 0.0;
    float3 pos = ro + float3(0.,0.,-t);
    bool skip = false;
    for( int i=0; i<16; i++ )
    {
        pos = ro + float3(0.,0.,-t);
        
        total = sampleSegments(pos);
        
        float h = total.x;
        //float h = sdgSegment(pos,pa,pb,ra).x;
        
        if (pos.z < 0.)
        {
        	skip = true;
        	break;
        }
        
        if( h<0.0001 || t>tmax ) break;
        
        t += h;
    }

    // Coloring
    float3 nor = total.yzw;
    float3 col = float3(0., 0., 0.);
    float alpha = 0.0;
    
    if( t<tmax && !skip )
    {
        float3 pos = ro + float3(0.,0.,-t);
        nor.xy *= -1.;
        
        // Use polar coordinates for texture samples
        float2 uv = float2(atan2(nor.x, nor.z)/6.2832, 2.*nor.y/3.)+.5;
        uv.x = frac(uv.x-uTime*.1);
        
        // Sample noise and normal textures
        float2 noiseOffset1 = float2(uTime * 0.15 * 0.2, uTime * 0.05);
    	float2 noiseOffset2 = float2(uTime * 0.075, uTime * -0.15);

        float noise = tex2D(noiseSampler, uv * 2. + noiseOffset1) * tex2D(noiseSampler, uv * 2. + noiseOffset2);
        float4 normalSample = (tex2D(normalSampler, uv * 2. + noiseOffset1) + tex2D(normalSampler, uv * 2. + noiseOffset2)) / 2;
        
        // Blend normal sample with SDF normals
        nor *= 2.*normalSample.rgb + 1.;
        nor = normalize(nor);
        
		// Lighting
		float3 lightVector = normalize(float3(0.25, 0.3, 0.75));
		
		// Specular Highlight
        float3 specularColor = float3(0.8, 0.9, 1.0);
		float specular = saturate(specularBRDF(nor,lightVector,22));

        float3 lightingColor = pow(specular * specularColor, 1.8);

		// Get fresnel color
		float fresnel = saturate(1 - nor.z);
		float3 fresnelCol = fresnelColor(nor) * fresnel * 0.3;
		
		// Diffuse and subsurface lighting
		float wrap = 1.0;
		float diffuse = max(0, dot(nor, lightVector));
		float subsurface = max(0, dot(nor, lightVector) + wrap) / (1 + wrap);
		
		// Make sure only subsurface approximation is shown since it looks cool
		float color = (subsurface - diffuse);
    
		// Quantize the color and lighting
		color = round(color * 16) / 16;
		lightingColor = round(lightingColor * 3) / 3;
        fresnelCol = round(fresnelCol * 12) / 12;
		
		// Get actual color and transparency
		float3 diffuseColor = float3(0.0, 0.2, 1.0); 
		alpha = saturate(specular * 0.9 + fresnel + color * 2.5) + 0.2;
		col = color * diffuseColor + lightingColor + fresnelCol;
		
		
    }
    

    // Gamma correction        
    col = sqrt( col );
    //alpha = sqrt( alpha );

	return float4(col * alpha, alpha);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}