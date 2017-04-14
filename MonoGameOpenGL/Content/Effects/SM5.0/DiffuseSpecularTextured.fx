float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float4x4 LightViewProj;
float4 AmbientLightColor;
float AmbientLightIntensity;

float4 DiffuseColor;
float DiffuseColorIntensity;

float TextureIntensity;

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

float Shininess;
float4 SpecularLightColor;
float SpecularLightIntensity;
float3 ViewVector;

float3 CameraPosition;
float3 CameraDirection;

const float DepthBias = 0.02;
float2 ShadowMapSize = (2048, 2048);
float3 fogColor;
float c;
float b;
bool FogEnabled;
bool ShadowsEnabled;
texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D ShadowMap;
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
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
    float4 WorldPos : COLOR1;
};

float3 ApplyFog(in float3 rgb, in float distance, in float3 cameraPosition, in float3 cameraDir)
{
    if (!FogEnabled)
        return rgb;
	
    float fogAmount = c * exp(-cameraPosition.y * b) * (1.0 - exp(-distance * cameraDir.y * b)) / cameraDir.y;
	
    return lerp(rgb, fogColor, fogAmount);

}

float4 GetPointLightContrbution(float3 lightPos, float3 worldPos, float3 normal, float4 color, float fallOffStart, float fallOffEnd, float intensity)
{
	 float pointLight1Distance = length(worldPos - lightPos);
    float pointLight1Attenuation = 1 - saturate((pointLight1Distance - fallOffStart) / fallOffEnd);
    pointLight1Attenuation = pow(pointLight1Attenuation, 2);
    float pointLight1Intensity = dot(normal, normalize((lightPos - worldPos)));
    return saturate(color * pointLight1Intensity * pointLight1Attenuation * intensity);
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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.WorldPos = worldPosition;
    output.Normal = normalize(mul(input.Normal, World));;
    output.TextureCoordinate = input.TextureCoordinate;


   
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

    float lightIntensity = dot(input.Normal, DiffuseLightDirection);
    float lightIntensity2 = dot(input.Normal, Diffuse2LightDirection);
    float lightIntensity3 = dot(input.Normal, Diffuse3LightDirection);

    float4 keyLight = saturate(DiffuseLightColor * DiffuseLightIntensity * lightIntensity);
    float4 fillLight = saturate(Diffuse2LightColor * Diffuse2LightIntensity * lightIntensity2);
    float4 backLight = saturate(Diffuse3LightColor * Diffuse3LightIntensity * lightIntensity3);


   //float4 GetPointLightContrbution(float3 lightPos, float3 worldPos, float3 normal, float4 color, float fallOffStart, float fallOffEnd, float intensity)
    float4 pointLight1 = GetPointLightContrbution(Point1Position, input.WorldPos, input.Normal, Point1Color, Point1FullPowerDistance, Point1FallOffDistance, Point1Intensity);
	float4 pointLight2 = GetPointLightContrbution(Point2Position, input.WorldPos, input.Normal, Point2Color, Point2FullPowerDistance, Point2FallOffDistance, Point2Intensity);
float4 pointLight3 = GetPointLightContrbution(Point3Position, input.WorldPos, input.Normal, Point3Color, Point3FullPowerDistance, Point3FallOffDistance, Point3Intensity);
float4 pointLight4 = GetPointLightContrbution(Point4Position, input.WorldPos, input.Normal, Point4Color, Point4FullPowerDistance, Point4FallOffDistance, Point4Intensity);


    float4 diffColor = saturate(keyLight + fillLight + backLight + pointLight1 + pointLight2 + pointLight3 + pointLight4);


    float3 light = normalize(DiffuseLightDirection);
    float3 normal = normalize(input.Normal);
    float3 r = normalize(2 * dot(light, normal) * normal - light);
    float3 v = normalize(mul(normalize(ViewVector), World));
    float dotProduct = dot(r, v);

    float4 specular = SpecularLightIntensity * SpecularLightColor * max(pow(dotProduct, Shininess), 0) * length(diffColor);
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate) * TextureIntensity;
    textureColor.a = 1;

	///SHADOW
    float shadowContribution = 1;
    if (ShadowsEnabled)
    {
        float NdotL = dot(normal, light);
        float4 lightingPosition = mul(input.WorldPos, LightViewProj);
    //Find the position in the shadow map for this pixel
        float2 ShadowTexCoord = mad(0.5f, lightingPosition.xy / lightingPosition.w, float2(0.5f, 0.5f));
        ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;
	// Get the current depth stored in the shadow map
        float ourdepth = (lightingPosition.z / lightingPosition.w);
        shadowContribution = CalcShadowTermPCF(ourdepth, NdotL, ShadowTexCoord);
    }
    
	///SHADOW
	


    float4 finalColor = saturate((textureColor + (DiffuseColor * DiffuseColorIntensity)) * (diffColor) + specular);
    finalColor = finalColor * shadowContribution + (AmbientLightColor * AmbientLightIntensity);
	
    float distanceFromCamera = length(CameraPosition - input.WorldPos);
    float3 colorAfterFog = ApplyFog(finalColor.rgb, distanceFromCamera, CameraPosition, CameraDirection);
	
    return float4(colorAfterFog, finalColor.w);

}

technique Textured
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}