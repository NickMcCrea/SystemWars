float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

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

texture ModelTexture;
sampler2D textureSampler = sampler_state {
	Texture = (ModelTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
	float2 TextureCoordinate : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
		float4 viewPosition = mul(worldPosition, View);
		output.Position = mul(viewPosition, Projection);

	float4 normal = normalize(mul(input.Normal, WorldInverseTranspose));
		float lightIntensity = dot(normal, DiffuseLightDirection);
	output.Color = saturate(DiffuseLightColor * DiffuseLightIntensity * lightIntensity);

	output.Normal = normal;

	output.TextureCoordinate = input.TextureCoordinate;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 light = normalize(DiffuseLightDirection);
	float3 normal = normalize(input.Normal);
	float3 r = normalize(2 * dot(light, normal) * normal - light);
	float3 v = normalize(mul(normalize(ViewVector), World));
	float dotProduct = dot(r, v);

	float4 specular = SpecularLightIntensity * SpecularLightColor * max(pow(dotProduct, Shininess), 0) * length(input.Color);

		float4 textureColor = tex2D(textureSampler, input.TextureCoordinate) * TextureIntensity;
		textureColor.a = 1;

	return saturate((textureColor + (DiffuseColor * DiffuseColorIntensity)) * (input.Color ) + AmbientLightColor * AmbientLightIntensity + specular);
}

technique Textured
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}