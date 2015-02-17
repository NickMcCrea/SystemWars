
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

bool FogEnabled = false;
float FogStart = 0;
float FogEnd = 50000;
float4 FogColor = float4(0.5f, 0.5f, 0.5f, 1.0f);	

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
	float FogFactor : FOG;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	float4x4 WV = mul(World,View);
	float4 viewpos = mul( input.Position, WV );

    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
	output.PositionWorld = mul(input.Position, World);
	output.Color = input.Color;
	output.FogFactor = saturate((FogEnd - length(viewpos)) / (FogEnd - FogStart));
    return output;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float3 normal = cross(ddy(input.PositionWorld.xyz), ddx(input.PositionWorld.xyz));
	normal = normalize(normal);
	float lightIntensity = dot(normal, DiffuseLightDirection);
	float4 diffuse = lightIntensity * DiffuseLightColor * DiffuseLightIntensity;
	float4 ambient = AmbientLightColor * AmbientLightIntensity;
	
	    // Set the color of the fog to grey.
    
	float4 finalColor = ((input.Color * ColorSaturation) + diffuse + ambient);

	if(FogEnabled)
		return input.FogFactor * finalColor + (1.0 - input.FogFactor) * FogColor;
	else
		return finalColor;

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
