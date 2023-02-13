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

bool Flat;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;


    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = Tiles.Sample(TilesSampler, input.TexCoord);
   // float dotColor = dot(color.rgb, float3(1, 1, 1));

    clip(color.a - 0.05);

    return color;
}

technique Good
{
    pass P0
    {
        AlphaBlendEnable = TRUE;
        // DestBlend = INVSRCALPHA;
        // SrcBlend = SRCALPHA;
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};