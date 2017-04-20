#include "Common.fx"

float4x4 WorldInverseTranspose;
float4x4 LightViewProj;

float4 DiffuseColor;
float DiffuseColorIntensity;

float Shininess;
float4 SpecularLightColor;
float SpecularLightIntensity;

float3 ViewVector;


float TextureIntensity;
texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
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