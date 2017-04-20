#include "Common.fx"

float3 CameraPosition;
float3 CameraDirection;



	
cbuffer cbPerObject 
{
	float4x4 World;
	float4x4 View;
	float4x4 Projection;
	float4x4 WorldViewProjection;
};
cbuffer cbAmbient
{
	float ColorSaturation;
	float4 AmbientLightColor;
	float AmbientLightIntensity;
}


float4x4 LightViewProj;


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
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
	output.PositionWorld = mul(input.Position, World);
	output.Color = input.Color;
    return output;
};




float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float3 normal = cross(ddy(input.PositionWorld.xyz), ddx(input.PositionWorld.xyz));
	normal = normalize(normal);
	float lightIntensity = dot(normal, DiffuseLightDirection);
    float lightIntensity2 = dot(normal, Diffuse2LightDirection);
    float lightIntensity3 = dot(normal, Diffuse3LightDirection);


   

    float shadowContribution = 1;

	///SHADOW
    if (ShadowsEnabled)
    {
        float NdotL = lightIntensity;
        float4 lightingPosition = mul(input.PositionWorld, LightViewProj);
		//Find the position in the shadow map for this pixel
        float2 ShadowTexCoord = mad(0.5f, lightingPosition.xy / lightingPosition.w, float2(0.5f, 0.5f));
        ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;
	// Get the current depth stored in the shadow map
        float ourdepth = (lightingPosition.z / lightingPosition.w);
        shadowContribution = CalcShadowTermPCF(ourdepth, NdotL, ShadowTexCoord);
    }
	///SHADOW

    float4 diffuseKey = lightIntensity * DiffuseLightColor * DiffuseLightIntensity;
    float4 diffuseFill = lightIntensity2 * Diffuse2LightColor * Diffuse2LightIntensity;
    float4 diffuseBack = lightIntensity3 * Diffuse3LightColor * Diffuse3LightIntensity;

     float4 pointLight1 = GetPointLightContrbution(Point1Position, input.PositionWorld, normal, Point1Color, Point1FullPowerDistance, Point1FallOffDistance, Point1Intensity);
	float4 pointLight2 = GetPointLightContrbution(Point2Position, input.PositionWorld,normal, Point2Color, Point2FullPowerDistance, Point2FallOffDistance, Point2Intensity);
float4 pointLight3 = GetPointLightContrbution(Point3Position, input.PositionWorld, normal, Point3Color, Point3FullPowerDistance, Point3FallOffDistance, Point3Intensity);
float4 pointLight4 = GetPointLightContrbution(Point4Position, input.PositionWorld, normal, Point4Color, Point4FullPowerDistance, Point4FallOffDistance, Point4Intensity);


    float4 diffuseFinal = saturate(diffuseKey + diffuseFill + diffuseBack + pointLight1 + pointLight2 + pointLight3 + pointLight4);

	float4 diffuse = diffuseFinal * (input.Color * ColorSaturation);
	float4 ambient = AmbientLightColor * AmbientLightIntensity;
	float4 finalColorBeforeFog = saturate(diffuse * shadowContribution + ambient);
	
	
	float distanceFromCamera = length(CameraPosition - input.PositionWorld);
	float3 colorAfterFog = ApplyFog(finalColorBeforeFog.rgb,distanceFromCamera, CameraPosition, CameraDirection);
	
	return float4(colorAfterFog, finalColorBeforeFog.w);
};

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
