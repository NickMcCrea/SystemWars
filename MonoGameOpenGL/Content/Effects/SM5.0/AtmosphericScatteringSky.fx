float4x4 World;
float4x4 View;
float4x4 Projection;


uniform float3 v3CameraPos;		
uniform float3 v3LightPos;		
uniform float3 v3InvWavelength;	
uniform float fCameraHeight;	 
uniform float fCameraHeight2;	
uniform float fOuterRadius;		
uniform float fOuterRadius2;	
uniform float fInnerRadius;		
uniform float fInnerRadius2;	
uniform float fKrESun;			
uniform float fKmESun;			
uniform float fKr4PI;			
uniform float fKm4PI;			
uniform float fScale;			
uniform float fScaleDepth;		
uniform float fScaleOverScaleDepth;	
uniform float g;                 
uniform float g2;                
const int nSamples = 2;
const float fSamples = 2.0;       

float4 v3Direction;

struct VertexShaderInput
{
    float4 Position : SV_POSITION;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
	float4 gl_FrontSecondaryColor : TEXCOORD0;
	float4 gl_FrontColor : TEXCOORD1;
	float3 v3Direction : TEXCOORD2;

};

float scale(float fCos)
{
	float x = 1.0 - fCos;
	return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	float3 v3Pos = input.Position.xyz;
	float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;

	bool inAtmosphere = false;

	if(fCameraHeight < fOuterRadius)
		inAtmosphere = true;

	float3 v3Start;
	float fStartOffset;
	if(!inAtmosphere)
	{
		// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
		float B = 2.0 * dot(v3CameraPos, v3Ray);
		float C = fCameraHeight2 - fOuterRadius2;
		float fDet = max(0.0, B*B - 4.0 * C);
		float fNear = 0.5 * (-B - sqrt(fDet));

		// Calculate the ray's starting position, then calculate its scattering offset
		v3Start = v3CameraPos + v3Ray * fNear;
		fFar -= fNear;
		float fStartAngle = dot(v3Ray, v3Start) / fOuterRadius;
		float fStartDepth = exp(-1.0 / fScaleDepth);
		fStartOffset = fStartDepth*scale(fStartAngle);
	}
	else
	{
		v3Start = v3CameraPos;
		float fHeight = length(v3Start);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
		float fStartAngle = dot(v3Ray, v3Start) / fHeight;
		fStartOffset = fDepth*scale(fStartAngle);
	}

    // Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;
	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

	// Now loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	for(int i=0; i<nSamples; i++)
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		float fLightAngle = dot(v3LightPos, v3SamplePoint) / fHeight;
		float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
		float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
		float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}

	// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
	output.gl_FrontSecondaryColor.rgb = v3FrontColor * fKmESun;
	output.gl_FrontColor.rgb = v3FrontColor * (v3InvWavelength * fKrESun);
	output.v3Direction = v3CameraPos - v3Pos;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float fCos = dot(v3LightPos,input.v3Direction) / length(input.v3Direction);
	float fMiePhase = 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos*fCos) / pow(1.0 + g2 - 2.0*g*fCos, 1.5);
    float4 gl_FragColor = input.gl_FrontColor + fMiePhase * input.gl_FrontSecondaryColor;
	gl_FragColor.a = gl_FragColor.b;
	return gl_FragColor;
}

technique Technique1
{
    pass Pass1
    {
	    ZEnable = true;
		ZWriteEnable = false;
		CullMode = CCW;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}
