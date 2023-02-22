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

float3 LightPositions[MAX_LIGHTS];
float4 LightColors[MAX_LIGHTS];
float LightRadii[MAX_LIGHTS];
float LightIntensities[MAX_LIGHTS];
int LightCount = 0;

float AmbientLevel = 0.3;
float ShadingLevel = 15;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 TexCoord : TEXCOORD0;
    float Shading : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float Shading : COLOR0;
};

float4 CalcPointLight(int index, const in float3 worldPos)
{
    const float3 lightDirection = worldPos - LightPositions[index];
    const float distance = length(lightDirection);

    if (distance > LightRadii[index])
    {
        return float4(0, 0, 0, 0);
    }

    const float intensity = LightIntensities[index];
    const float distanceFactor = 1 - distance / LightRadii[index];
    const float attenuation = intensity * distanceFactor; // linear attenuation

    return LightColors[index] * attenuation;
}

VertexShaderOutput MainVS(const in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    output.WorldPosition = mul(input.Position, World);
    output.Shading = input.Shading;
    return output;
}

float4 MainPS(const in VertexShaderOutput input, bool flatPass) : COLOR
{
    float4 color = Tiles.Sample(TilesSampler, input.TexCoord);

    // apply transparency in flat pass
    if (flatPass)
    {
        // clip transparent pixels as not to fill depth buffer
        clip(color.a - 0.05);
    }
    
    // apply shading
    const float brightness = 1 - input.Shading * (ShadingLevel / 31.0);
    color = float4(color.rgb * brightness, color.a);

    // compute lighting
    float4 lightTotal = float4(AmbientLevel, AmbientLevel, AmbientLevel, 1);
    for (int i = 0; i < LightCount; i++)
    {
        lightTotal += CalcPointLight(i, input.WorldPosition);
    }

    return color * lightTotal;
}

float4 OpaquePS(const in VertexShaderOutput input) : COLOR
{
    return MainPS(input, false);
}

float4 FlatPS(const in VertexShaderOutput input) : COLOR
{
    return MainPS(input, true);
}

technique Faces
{
    pass Opaque
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL OpaquePS();
    }
    pass Flat
    {
        AlphaBlendEnable = TRUE;
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL FlatPS();
    }
};