#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

#define MAX_LIGHTS 16

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
float4 TileUv = float4(1, 1, 0, 0); // debug: tile coordinates are scaled by xy and then offset by zw
float4 FlatTileUv = float4(1, 1, 0, 0); // the same, for flat (transparent) faces
float FlatCutoff = 0.05; // flat faces: pixels with an alpha below this are dropped; at 0.5 or more it is an alpha test (the surviving pixels are opaque)
float SharpTexels = 0; // 1: sharp bilinear filtering (use with a linear sampler)
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
    const float4 tileUv = flatPass ? FlatTileUv : TileUv;
    float2 uv = input.TexCoord.xy * tileUv.xy + tileUv.zw;

    if (SharpTexels > 0)
    {
        // "sharp bilinear": every texel stays flat in its middle and is blended with its neighbours only over about one screen pixel
        // at the boundary (a screen pixel is fwidth(t) texels). Magnified like this it looks crisp without the stair-steps of point sampling.
        const float2 t = uv * 64.0;
        const float2 pixelInTexels = fwidth(t);
        const float2 region = saturate(0.5 - 0.5 * pixelInTexels);
        const float2 p = floor(t) + clamp(t - floor(t), region, 1 - region);
        uv = p / 64.0;
    }

    float4 color = Tiles.Sample(TilesSampler, float3(uv, input.TexCoord.z));

    // apply transparency in flat pass
    if (flatPass)
    {
        // clip transparent pixels as not to fill depth buffer
        clip(color.a - FlatCutoff);
        if (FlatCutoff >= 0.5)
        {
            color.a = 1;
        }
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

float4 OutlinePS(const in VertexShaderOutput input) : COLOR
{
    // debug: solid colour, used with a wireframe rasterizer to show where the geometry of a face is
    return float4(1, 0, 1, 1);
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
    pass Outline
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL OutlinePS();
    }
};