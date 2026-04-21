// Normal sprite, but semi-occluded by tile target.

#pragma warning (disable : 4717)

float4x4 World;
float4x4 View;
float2 MaskSize;
float2 MaskOffset;
float2 ScreenPosition;
float2 ScreenResolution;

texture2D Texture;
texture2D Glowmask;
texture2D Tiles; 
texture2D Light; 
texture2D Black; 
sampler TextureSampler : register(s0) = sampler_state { Texture = (Texture); };
sampler GlowmaskSampler : register(s1) = sampler_state { Texture = (Glowmask); };
sampler LightSampler : register(s2) = sampler_state { Texture = (Light); };
sampler TilesSampler : register(s3) = sampler_state { Texture = (Tiles); };
sampler BlackSampler : register(s4) = sampler_state { Texture = (Black); };

struct VertexIn
{
    float4 Color : COLOR0;
    float2 Uv : TEXCOORD0;
    float4 Position : SV_POSITION;
};
struct VertexOut
{
    float4 Color : COLOR0;
	float2 UvTexture : TEXCOORD0;
	float2 UvLight : TEXCOORD1;
	float2 UvTiles : TEXCOORD2;
    float4 Position : SV_POSITION;
};

VertexOut SpriteVertexShader(in VertexIn input)
{
    float4 worldPos = mul(input.Position, World);

    VertexOut output = (VertexOut)0;
    output.Position = mul(worldPos, View);
    output.UvTexture = input.Uv;
    output.UvLight = worldPos.xy / ScreenResolution;
    output.UvTiles = (worldPos.xy - MaskOffset) / MaskSize;
    output.Color = input.Color;
    return output;
}
float4 SpritePixelShader(VertexOut input) : SV_TARGET0
{
	float4 glow = tex2D(GlowmaskSampler, input.UvTexture);
	float4 tiles = tex2D(TilesSampler, input.UvTiles);
	float4 black = tex2D(BlackSampler, input.UvTiles);
    if (tiles.a > 0.1 || black.a > 0.1)
    {
        // Red tint underground.
        return glow * float4(0.09, 0.03, 0.00, 0.09) * input.Color;
    }

	float4 albedo = tex2D(TextureSampler, input.UvTexture);
    float4 light = tex2D(LightSampler, input.UvLight);
	return ((albedo * light) + glow) * input.Color;
}

technique SpriteBatch
{
	pass
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}