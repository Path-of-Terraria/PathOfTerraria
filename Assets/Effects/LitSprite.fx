// SpriteBatch sprite lit by a lighting buffer.

#pragma warning (disable : 4717)

matrix MatrixTransform;
float2 ScreenResolution;

texture2D Texture;
texture2D Lighting; 
sampler TextureSampler = sampler_state { Texture = (Texture); };
sampler LightingSampler = sampler_state { Texture = (Lighting); };

struct VertexInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};
struct PixelInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
	float2 UvTexture : TEXCOORD0;
	float2 UvLight : TEXCOORD1;
};

PixelInput SpriteVertexShader(in VertexInput input)
{
    float4 worldPos = input.Position;

    PixelInput output = (PixelInput)0;
    output.Position = mul(worldPos, MatrixTransform);
    output.UvTexture = input.TexCoord;
    output.UvLight = worldPos.xy / ScreenResolution;
    output.Color = input.Color;
    return output;
}
float4 SpritePixelShader(PixelInput input) : SV_TARGET0
{
	float4 albedo = tex2D(TextureSampler, input.UvTexture);
    float4 light = tex2D(LightingSampler, input.UvLight);
	return albedo * light * input.Color;
}

technique SpriteBatch
{
	pass
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}