float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float4x4 LightViewProj;
float4 AmbientLightColor;
float AmbientLightIntensity ;

float4 DiffuseColor;
float DiffuseColorIntensity;

float TextureIntensity;

float4 DiffuseLightDirection;
float4 DiffuseLightColor;
float DiffuseLightIntensity;

float Shininess;
float4 SpecularLightColor;
float SpecularLightIntensity;

float3 ViewVector;

const float DepthBias = 0.02;
float2 ShadowMapSize = (512,512);

texture ModelTexture;
sampler2D textureSampler = sampler_state {
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
	float4 Color : COLOR0;
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

	float3 normal = normalize(mul(input.Normal, World));
	output.Normal = normal;
	output.WorldPos = worldPosition;
	float lightIntensity = dot(normal, DiffuseLightDirection);
	output.Color = saturate(DiffuseLightColor * DiffuseLightIntensity * lightIntensity);


	output.TextureCoordinate = input.TextureCoordinate;
	return output;
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
	float3 light = normalize(DiffuseLightDirection);
	float3 normal = normalize(input.Normal);
	float3 r = normalize(2 * dot(light, normal) * normal - light);
	float3 v = normalize(mul(normalize(ViewVector), World));
	float dotProduct = dot(r, v);
	
    float NdotL = dot(light,normal);
	
	float4 lightingPosition = mul(input.WorldPos, LightViewProj);
    //Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = mad(0.5f , lightingPosition.xy / lightingPosition.w , float2(0.5f, 0.5f));
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

	// Get the current depth stored in the shadow map
    float ourdepth = (lightingPosition.z / lightingPosition.w);

    float shadowContribution = CalcShadowTermPCF(ourdepth, NdotL, ShadowTexCoord);

	float4 specular = SpecularLightIntensity * SpecularLightColor * max(pow(dotProduct, Shininess), 0) * length(input.Color);

	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate) * TextureIntensity;
	textureColor.a = 1;

	float4 finalColor = saturate((textureColor + (DiffuseColor * DiffuseColorIntensity)) * (input.Color ) + AmbientLightColor * AmbientLightIntensity + specular);
return finalColor * shadowContribution;

}

technique Textured
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}