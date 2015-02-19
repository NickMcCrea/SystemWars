float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewProjection;

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
	float4 Color : COLOR0;
	float4 PositionWorld : NORMAL0;
	float4 gl_FrontSecondaryColor : TEXCOORD0;
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

	// Get the ray from the camera to the vertex and its length
	float3 v3Pos = mul(input.Position.xyz, World);
	float3 v3Ray = v3Pos - v3CameraPos;
	float fFar = length(v3Ray);
	v3Ray /= fFar;

	// Calculate the farther intersection of the ray with the outer atmosphere (which is the far point of the ray passing through the atmosphere)
	float B = 2.0 * dot(v3CameraPos, v3Ray);
	float C = fCameraHeight2 - fOuterRadius2;
	float fDet = max(0.0, B*B - 4.0 * C);
	fFar = 0.5 * (-B + sqrt(fDet));

	float fNear;
	float3 v3Start;

	if (fCameraHeight < fOuterRadius)
	{
		v3Start = v3CameraPos;
	}
	else
	{
		fNear = 0.5 * (-B - sqrt(fDet));
		v3Start = v3CameraPos + v3Ray*fNear;
		fFar -= fNear;
	}


	// Calculate attenuation from the camera to the top of the atmosphere toward the vertex
	float fHeight = length(v3Start);
	float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
	float fAngle = dot(v3Ray, v3Start) / fHeight;
	float fScatter = fDepth*scale(fAngle);
	output.gl_FrontSecondaryColor.rgb = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
	
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
	return diffuseFinal + input.gl_FrontSecondaryColor;
}

technique Technique1
{
	pass Pass1
	{
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}
