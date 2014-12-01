// Globals
float4x4 World;
float4x4 WorldViewProjection;
float3 DiffuseLightDirection;
float AmbientLightIntensity;

SamplerState TextureSampler
{
  Filter = MIN_MAG_MIP_Linear;
  AddressU = WRAP;
  AddressV = WRAP;
};

// Structs
struct VS_INPUT
{
  float3 Position : SV_POSITION;
  float3 Normal   : NORMAL0;
  float2 UV       : TEXCOORD0;
};

struct PS_INPUT
{
  float4 Position : SV_POSITION;
  float3 Normal   : NORMAL0;
  float2 UV       : TEXCOORD0;
};

// Vertex Shader
PS_INPUT VS_Main(VS_INPUT input)
{
  PS_INPUT output = (PS_INPUT)0;

  // Multiply each vertex with the world view projection matrix.
  output.Position = mul(float4(input.Position, 1.0f), WorldViewProjection);
  output.Normal = mul(input.Normal, (float3x3)World);

  // Pass on the texture coordinates to the pixel shader struct.
  output.UV = input.UV;

  return output;
}

// Pixel Shader
float4 PS_Main(PS_INPUT input) : SV_TARGET
{
  float4 output = float4(0.0f, 0.0f, 0.0f, 0.0f);

  // Calculate the amount of lighting.
  float lighting = saturate(dot(-DiffuseLightDirection, input.Normal)) + AmbientLightIntensity;

  // Combine the diffuse color with the amount of lighting.
  output = float4((float3(1, 1, 1) * lighting), 1);

  return output;
}

// Technique
technique Main
{
  pass p0
  {
	    VertexShader = compile vs_5_0  VS_Main();
        PixelShader = compile ps_5_0 PS_Main();
  }
}