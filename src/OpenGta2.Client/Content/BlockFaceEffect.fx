#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

#define MAX_LIGHTS 6


matrix World;
matrix WorldViewProjection;

Texture2DArray Tiles : register(t0);
sampler TilesSampler : register(s0);

bool Flat;

float3 LightPositions[MAX_LIGHTS];
float4 LightColors[MAX_LIGHTS];
float LightRadii[MAX_LIGHTS];
float LightIntensities[MAX_LIGHTS];
int LightCount = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
};

VertexShaderOutput MainVS(const in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    output.WorldPosition = mul(input.Position, World);
    return output;
}

float4 CalcPointLight(int index, float3 worldPos)
{
    float3 lightDirection = worldPos - LightPositions[index];
    const float distance = length(lightDirection);

    if (distance > LightRadii[index])
    {
        return float4(0, 0, 0, 0);
    }

    const float intensity = LightIntensities[index];
    const float distanceFactor = 1 - distance / LightRadii[index];
    const float attenuation = intensity * distanceFactor;// linear attenuation

    return LightColors[index] * attenuation;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = Tiles.Sample(TilesSampler, input.TexCoord);

    if (Flat)
    {
        clip(color.a - 0.05);
    }

    // 0.3 is ambient level proved by mapscript
    // TODO: ambient level should be provided to shader
    float4 lightTotal = float4(0.3, 0.3, 0.3, 1);

    for (int i = 0; i < LightCount; i++)
    {
        lightTotal += CalcPointLight(i, input.WorldPosition);
    }

    return color * lightTotal;
}

technique Faces
{
    pass P0
    {
        AlphaBlendEnable = TRUE;
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};