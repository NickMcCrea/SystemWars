
cbuffer cbPerObject 
{
	float4x4 World;
	float4x4 View;
	float4x4 Projection;
	float4x4 WorldViewProjection;
};

float4 ApexColor;
float4 CenterColor;


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
	in general, daytime sky color is a gradual fade from "horizon color" to "zenith color" overheadPhotograph of sky color
for example, a clear blue sky usually goes from a light cyan at the horizon to a rich medium-blue
at dusk and dawn, there is an additional fade of warm color from a broad stretch of the east/west horizon to the zenith, as shown in the photograph to the right
by choosing an attractive set of colors, you can just interpolate between them for all times of day and night
the colors of a sunset are typically: yellow at the edge, then orange-red, then purple-slate blue

*/

    // Determine the position on the sky dome where this pixel is located.
    height = input.PositionWorld.y;
	eastwest = input.PositionWorld.x;

    // The value ranges from -1.0f to +1.0f so change it to only positive values.
    if(height < 0.0)
    {
        height = 0.0f;
    }

    // Determine the gradient color by interpolating between the apex and center based on the height of the pixel in the sky //dome.
    altitudeColor = lerp(CenterColor, ApexColor, height);

	float2 p = input.PositionWorld.xy;
	float2 o = float2(0,0);
	float c = 0.1/(length(p-o));
	float4 col = float4(c, c, 0.4 + c, 1);
	
    return col;
	
};

technique Technique1
{
    pass Pass1
    {
		
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
	
}
