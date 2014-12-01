
cbuffer cbPerObject 
{
	float4x4 World;
	float4x4 View;
	float4x4 Projection;
	float4x4 WorldViewProjection;
};

// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

   
    output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;
    
    return output;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

    return input.Color;
};

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_4_0_level_9_1  VertexShaderFunction();
        PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
    }
}
