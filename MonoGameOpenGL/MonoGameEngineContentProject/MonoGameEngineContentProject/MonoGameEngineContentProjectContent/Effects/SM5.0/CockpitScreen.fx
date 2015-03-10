
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

float time;

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

	float2 sp = input.PositionWorld.xy;//vec2(.4, .7);
	float2 p = sp*6.0 - float2(125.0,125.0);
	float2 i = p;
	float c = 1.0;
	
	float inten = 0.01;

	for (int n = 0; n < 4; n++) 
	{
		float t = time/10000.0* (1.0 - (3.0 / float(n+1)));
		i = p + float2(cos(t - i.x) + sin(t + i.y), sin(t - i.y) + cos(t + i.x));
		c += 1.0/length(float2(p.x / (sin(i.x+t)/inten),p.y / (cos(i.y+t)/inten)));
	}
	c /= float(4);
	c = 1.5-sqrt(c);
	float4 effectColor = 0.2 * (float4(float3(c*c*c*c,c*c*c*c,c*c*c*c), 999.0) + float4(0.0, 0.3, 0.5, 1.0));
	effectColor.w = 0.01f;


	float3 normal = cross(ddy(input.PositionWorld.xyz), ddx(input.PositionWorld.xyz));
	normal = normalize(normal);
	float lightIntensity = dot(normal, DiffuseLightDirection);
	float4 diffuse = lightIntensity * DiffuseLightColor * DiffuseLightIntensity * (input.Color * ColorSaturation);
	float4 ambient = AmbientLightColor * AmbientLightIntensity;
	float4 final = saturate(diffuse + ambient);
	final /= 10;
	final.w = 0.01f;
	return saturate ( effectColor + final);
};

technique Technique1
{
	pass Pass1
	{
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = CW;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}

}
