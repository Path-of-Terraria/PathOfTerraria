// Normal sprite, but semi-occluded by tile target.

#pragma warning (disable : 4717)

float4x4 World;
float4x4 View;
float2 MaskSize;
float2 MaskOffset;
float2 ScreenPosition;
texture2D Texture;
sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
};
texture2D Glowmask;
sampler GlowmaskSampler : register(s1) = sampler_state
{
	Texture = (Glowmask);
};
texture2D Tiles; 
sampler TilesSampler : register(s2) = sampler_state
{
	Texture = (Tiles);
};
texture2D Black; 
sampler BlackSampler : register(s3) = sampler_state
{
	Texture = (Black);
};

struct VertexIn
{
    float4 Color : COLOR0;
    float2 Uv : TEXCOORD0;
    float4 Position : SV_POSITION;
};
struct VertexOut
{
    float4 Color : COLOR0;
	float2 Uv0 : TEXCOORD0;
	float2 Uv1 : TEXCOORD1;
    float4 Position : SV_POSITION;
};

VertexOut SpriteVertexShader(in VertexIn input)
{
    float4 worldPos = mul(input.Position, World);

    VertexOut output = (VertexOut)0;
    output.Position = mul(worldPos, View);
    output.Uv0 = input.Uv;
    output.Uv1 = (worldPos.xy - MaskOffset) / MaskSize;
    output.Color = input.Color;
    return output;
}
float4 SpritePixelShader(VertexOut input) : SV_TARGET0
{
	float4 diffuse = tex2D(TextureSampler, input.Uv0) * input.Color;
	float4 glow = tex2D(GlowmaskSampler, input.Uv0);
	float4 tiles = tex2D(TilesSampler, input.Uv1);
	float4 black = tex2D(BlackSampler, input.Uv1);
    if (tiles.a > 0.1 || black.a > 0.1)
    {
        // Red tint underground.
        return glow * float4(0.09, 0.03, 0.00, 0.09);
    }

	return diffuse + glow;
}

technique SpriteBatch
{
	pass
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}