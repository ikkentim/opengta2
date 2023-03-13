#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

matrix MatrixTransform;
float4 Color = float4(1, 1, 1, 1);

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
    AddressU = clamp;
    AddressV = clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(const in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, MatrixTransform);
    output.TexCoord = input.TexCoord;
    return output;
}

float4 MainPS(const in VertexShaderOutput input) : COLOR
{
    return Texture.Sample(TextureSampler, input.TexCoord) * Color;
}

technique T0
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};