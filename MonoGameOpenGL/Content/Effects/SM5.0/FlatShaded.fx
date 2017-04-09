float3 CameraPosition;
float3 CameraDirection;
bool FogEnabled;
bool ShadowsEnabled;
float c;
float b;
float3 fogColor;
	
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
cbuffer cbDiffuse
{
	
	float4 DiffuseLightColor;
	float4 DiffuseLightDirection;
	float DiffuseLightIntensity;
}

float4 Diffuse2LightDirection;
float4 Diffuse2LightColor;
float Diffuse2LightIntensity;

float4 Diffuse3LightDirection;
float4 Diffuse3LightColor;
float Diffuse3LightIntensity;

float3 Point1Position;
float4 Point1Color;
float Point1Intensity;
float Point1FallOffDistance;
float Point1FullPowerDistance;

float3 Point2Position;
float4 Point2Color;
float Point2Intensity;
float Point2FallOffDistance;
float Point2FullPowerDistance;

float3 Point3Position;
float4 Point3Color;
float Point3Intensity;
float Point3FallOffDistance;
float Point3FullPowerDistance;

float3 Point4Position;
float4 Point4Color;
float Point4Intensity;
float Point4FallOffDistance;
float Point4FullPowerDistance;

float4x4 LightViewProj;
Texture2D ShadowMap;
const float DepthBias = 0.02;
float2 ShadowMapSize = (2048, 2048);
SamplerState ShadowMapSampler
{
	Texture = (ShadowMap);
	MinFilter = point;
	MagFilter = point;
	MipFilter = point;
	AddressU = Wrap;
	AddressV = Wrap;
};

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


float4 GetPointLightContrbution(float3 lightPos, float3 worldPos, float3 normal, float4 color, float fallOffStart, float fallOffEnd, float intensity)
{
	 float pointLight1Distance = length(worldPos - lightPos);
    float pointLight1Attenuation = 1 - saturate((pointLight1Distance - fallOffStart) / fallOffEnd);
    pointLight1Attenuation = pow(pointLight1Attenuation, 2);
    float pointLight1Intensity = dot(normal, normalize((lightPos - worldPos)));
    return saturate(color * pointLight1Intensity * pointLight1Attenuation * intensity);
}

float3 ApplyFog(in float3 rgb, in float distance, in float3 cameraPosition, in float3 cameraDir)
{
	if(!FogEnabled)
		return rgb;
	
	float fogAmount = c * exp(-cameraPosition.y * b) * (1.0-exp(-distance * cameraDir.y * b))/cameraDir.y;

	return lerp(rgb,fogColor,fogAmount);

}

float CalcShadowTermPCF(float light_space_depth, float ndotl, float2 shadow_coord)
{
	float shadow_term = 0;

	//float2 v_lerps = frac(ShadowMapSize * shadow_coord);

	float variableBias = clamp(0.001 * tan(acos(ndotl)), 0, DepthBias);

	//safe to assume it's a square
	float size = 1 / ShadowMapSize.x;

	float samples[4];
	samples[0] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord).r);
	samples[1] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(size, 0)).r);
	samples[2] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(0, size)).r);
	samples[3] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(size, size)).r);

	shadow_term = (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
	//shadow_term = lerp(lerp(samples[0],samples[1],v_lerps.x),lerp(samples[2],samples[3],v_lerps.x),v_lerps.y);

	return shadow_term;
}

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
