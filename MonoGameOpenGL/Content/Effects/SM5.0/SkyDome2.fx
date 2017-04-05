
cbuffer cbPerObject 
{
	float4x4 World;
	float4x4 View;
	float4x4 Projection;
	float4x4 WorldViewProjection;
};

float4 ApexColor;
float4 CenterColor;
float4 SunsetColor = float4(0.8,0.1,0.2,1);
float3 LightDirection;

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
	float4 PositionWorld : NORMAL0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
	output.PositionWorld = input.Position;
    return output;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float height;
    float eastwest;
	float4 altitudeColor;
/*
	in general, daytime sky color is a gradual fade from "horizon color" to "zenith color"
for example, a clear blue sky usually goes from a light cyan at the horizon to a rich medium-blue
at dusk and dawn, there is an additional fade of warm color from a broad stretch of the east/west horizon to the zenith, as shown in the photograph to the right
by choosing an attractive set of colors, you can just interpolate between them for all times of day and night
the colors of a sunset are typically: yellow at the edge, then orange-red, then purple-slate blue

*/

    // Determine the position on the sky dome where this pixel is located.
    height = input.PositionWorld.y;
	eastwest = input.PositionWorld.x;
	float sunHeight = LightDirection.y;
	
	float sizeOfSphere= length(input.PositionWorld);
	float3 lightPos = (normalize(LightDirection) * sizeOfSphere);
	
    // The value ranges from -1.0f to +1.0f so change it to only positive values.
    if(height < 0.0)
    {
        height = 0.0f;
    }
	if(sunHeight < 0)
	{
		sunHeight = 0;
	}
	
	
	
	//CenterColor = lerp(CenterColor, SunsetColor, (1-sunHeight));

    // Determine the gradient color by interpolating between the apex and center based on the height of the pixel in the sky //dome.
    altitudeColor = lerp(CenterColor, ApexColor, height);
	
	
	
	float dotAngle = acos(dot(input.PositionWorld, LightDirection));
	float c = 0.04 / dotAngle;
	float4 col = float4(c,c,c,0);
	
    return saturate(altitudeColor + col);
	
};

technique Technique1
{
    pass Pass1
    {
		
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
	
}
