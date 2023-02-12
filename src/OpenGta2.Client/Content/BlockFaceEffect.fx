#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

matrix WorldViewProjection;

Texture2DArray<float4> Tiles : register(t0);

sampler TilesSampler : register(s0);

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 TexCoord : TEXCOORD0;
    float Transparancy : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
    float Transparancy : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    output.Transparancy = input.Transparancy;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = Tiles.Sample(TilesSampler, input.TexCoord);

    if (input.Transparancy > 0)
    {
        clip(color.r + color.g + color.b - 0.05);
    }

    // return float4(input.TexCoord, 1);
    return float4(color.r, color.g, color.b, 1);
}

technique Good
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};