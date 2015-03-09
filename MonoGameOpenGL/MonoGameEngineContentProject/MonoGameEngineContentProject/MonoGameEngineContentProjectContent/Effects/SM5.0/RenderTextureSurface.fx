
cbuffer cbPerObject 
{
	float4x4 World;
	float4x4 View;
	float4x4 Projection;
	float4x4 WorldViewProjection;
};

Texture2D surfaceTexture;
float borderSize = 0.01f;
float4 borderColor = float4(0.1,0.2,0.1,1);

SamplerState MeshTextureSampler
{
    Filter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
	float2 Texture : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
	float2 Texture : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
	output.Texture = input.Texture;
    return output;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if(input.Texture.x < borderSize)
		return borderColor;

	if(input.Texture.x > (1-borderSize))
		return borderColor;

	if(input.Texture.y < borderSize)
		return borderColor;
	if(input.Texture.y > (1-borderSize))
		return borderColor;

	return surfaceTexture.Sample(MeshTextureSampler, input.Texture);
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
