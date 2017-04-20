#include "Common.fx"

float4x4 WorldViewProjection;
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

float3 GetWavePoint(float3 originalPos, float amplitude, float wavelength, float W, float Q, float2 direction, float timeFactor){

	float dotD = dot(originalPos.xz, direction);
	float C = cos(wavelength*dotD + timeFactor);
	float S = sin(wavelength*dotD + timeFactor);
	return float3(originalPos.x + Q*amplitude*C*direction.x, amplitude * S, originalPos.z + Q*amplitude*C*direction.y);
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	
	// Change the position vector to be 4 units for proper matrix calculations.
    input.Position.w = 1.0f;
	
	
	///WAVES
	// Gerstner Wave
	float3 P0 = input.Position.xyz;
	
	float A1 = 0.1;	// amplitude
	float L1 = 50;	// wavelength
	float W1 = 2*3.1416/L1; //phase constant
	float Q1 = 0.5;
	float2 D1 = float2(0, 1);
	
	float A2 = 0.2;	// amplitude
	float L2 = 50;	// wavelength
	float W2 = 2*3.1416/L2;
	float Q2 = 0.3;
	float2 D2 = float2(1, 0);

	float3 P1 = GetWavePoint(P0,A1,L1,W1,Q1,D1, time/1234);
	float3 P2 = GetWavePoint(P0,A2,L2,W2,Q2,D2,time/1873);
	float3 P3 = GetWavePoint(P0,0.3,30,W1,Q1, float2(-0.25,-0.75), time/2345);
	float3 P4 = GetWavePoint(P0,0.4,30,W1,Q1, float2(-0.45,0.212), time/2345);
	
	input.Position.xyz = P1 + P2 + P3 + P4;
	
	///WAVES
	
	
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
	
	return float4(colorAfterFog, 0.8);
};

technique Technique1
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
	
}
