float4x4 World;
float4x4 View;
float4x4 Projection;

cbuffer cbAmbient
{
	float ColorSaturation;
	float4 AmbientLightColor;
	float AmbientLightIntensity;
}
cbuffer cbDiffuse
{
	
	float4 DiffuseLightColor;
	float4 DiffuseLightDirection;
	float DiffuseLightIntensity;
}


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
const int nSamples = 2;
const float fSamples = 2.0;       


struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 gl_FrontSecondaryColor : TEXCOORD0;
	float4 gl_FrontColor : TEXCOORD1;
	float4 Color : COLOR0;
	float4 PositionWorld : NORMAL0;
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
	output.PositionWorld = mul(input.Position, World);
	output.Color = input.Color;

	float3 v3Pos = input.Position.xyz;
	float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;

	bool inAtmosphere = false;



	float3 v3Start;
	float fStartOffset;

	// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
	float B = 2.0 * dot(v3CameraPos, v3Ray);
	float C = fCameraHeight2 - fOuterRadius2;
	float fDet = max(0.0, B*B - 4.0 * C);
	float fNear = 0.5 * (-B - sqrt(fDet));

	// Calculate the ray's starting position, then calculate its scattering offset
	v3Start = v3CameraPos + v3Ray * fNear;
	fFar -= fNear;

	float fDepth = exp((fInnerRadius - fOuterRadius) / fScaleDepth);
	float fCameraAngle = dot(-v3Ray, v3Pos) / length(v3Pos);
	float fLightAngle = dot(v3LightPos, v3Pos) / length(v3Pos);
	float fCameraScale = scale(fCameraAngle);
	float fLightScale = scale(fLightAngle);
	float fCameraOffset = fDepth*fCameraScale;
	float fTemp = (fLightScale + fCameraScale);


	// Initialize the scattering loop variables
	float fSampleLength = fFar / fSamples;
	float fScaledLength = fSampleLength * fScale;

	float3 v3SampleRay = v3Ray * fSampleLength;
	float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

		// Now loop through the sample rays
	float3 v3FrontColor = float3(0.0, 0.0, 0.0);
	float3 v3Attenuate;

	for(int i=0; i<nSamples; i++)
	{
		float fHeight = length(v3SamplePoint);
		float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
		float fScatter = fDepth*fTemp - fCameraOffset;
		v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
		v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
		v3SamplePoint += v3SampleRay;
	}


	// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
	output.gl_FrontColor.rgb = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);

	// Calculate the attenuation factor for the ground
	output.gl_FrontSecondaryColor.rgb = v3Attenuate;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float3 normal = cross(ddy(input.PositionWorld.xyz), ddx(input.PositionWorld.xyz));
	normal = normalize(normal);
	float lightIntensity = dot(normal, DiffuseLightDirection);
	float4 diffuse = lightIntensity * DiffuseLightColor * DiffuseLightIntensity * (input.Color * ColorSaturation);
	float ambient = AmbientLightColor * AmbientLightIntensity;;
	float4 diffuseFinal = saturate(diffuse + ambient);
	return input.gl_FrontColor + diffuseFinal * input.gl_FrontSecondaryColor;
}

technique Technique1
{
	pass Pass1
	{
		AlphaBlendEnable = true;
		SrcBlend = One ;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}
