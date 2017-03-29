#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


matrix LightWorldViewProjection;


struct SMapVertexToPixel
{
	float4 Position : SV_POSITION;
	float4 Position2d : TEXCOORD0;
};

struct SMapPixelToFrame
{
	float4 Color : COLOR0;
};

SMapVertexToPixel MainVS(float4 inPos : SV_POSITION)
{
	SMapVertexToPixel output = (SMapVertexToPixel)0;
	output.Position = mul(inPos, LightWorldViewProjection);
	output.Position2d = output.Position;
	return output;
}

SMapPixelToFrame MainPS(SMapVertexToPixel input) : COLOR
{
	SMapPixelToFrame output = (SMapPixelToFrame)0;
	output.Color = input.Position2d.z / input.Position2d.w;
	return output;
}

technique ShadowMap
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};