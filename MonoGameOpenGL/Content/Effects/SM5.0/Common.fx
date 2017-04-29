float4x4 World;
float4x4 View;
float4x4 Projection;

float  time;

float3 CameraPosition;
float3 CameraDirection;

float ColorSaturation;
float4 AmbientLightColor;
float AmbientLightIntensity;

float4 DiffuseLightDirection;
float4 DiffuseLightColor;
float DiffuseLightIntensity;

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


bool FogEnabled;
float3 fogColor;
float c;
float b;
bool ShadowsEnabled;
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

     
    float variableBias = clamp(0.001 * tan(acos(ndotl)), 0, DepthBias);

    //safe to assume it's a square
    float size = 1 / ShadowMapSize.x;
    	
    float samples[4];
    samples[0] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord).r);
    samples[1] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(size, 0)).r);
    samples[2] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(0, size)).r);
    samples[3] = (light_space_depth - variableBias < ShadowMap.Sample(ShadowMapSampler, shadow_coord + float2(size, size)).r);

    shadow_term = (samples[0] + samples[1] + samples[2] + samples[3]) / 4.0;
    	
    return shadow_term;
}

float4 GetPointLightContrbution(float3 lightPos, float3 worldPos, float3 normal, float4 color, float fallOffStart, float fallOffEnd, float intensity)
{
	 float pointLight1Distance = length(worldPos - lightPos);
    float pointLight1Attenuation = 1 - saturate((pointLight1Distance - fallOffStart) / fallOffEnd);
    pointLight1Attenuation = pow(pointLight1Attenuation, 2);
    float pointLight1Intensity = dot(normal, normalize((lightPos - worldPos)));
    return saturate(color * pointLight1Intensity * pointLight1Attenuation * intensity);
}