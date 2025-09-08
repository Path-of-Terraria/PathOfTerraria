float scroller;
texture sampleTexture;

sampler2D sampler0 = sampler_state
{
	texture = <sampleTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = wrap;
	AddressV = wrap;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float comb = input.TextureCoordinates.x + input.TextureCoordinates.y;
	float4 color = tex2D(sampler0, input.TextureCoordinates);
	float alpha = color.a;
	float lum = lerp(((0.9 * color.r) + (1.8 * color.g) + (0.3 * color.b)) / 3.0, 1, 0.3f);
	
	lum = round(lum * 10) / 10;
	
	if ((input.TextureCoordinates.x + input.TextureCoordinates.y + scroller) % 1 < 0.05)
	{
		lum = 1;
	}
	
	color = lerp(lerp(float4(0.12549, 0.21568, 114, 1), float4(0.6, 0.6667, 0.7776, 1), lum), lerp(float4(0.6, 0.6667, 0.7776, 1), float4(1, 1, 1, 1), lum), lum);
	color *= alpha;
	return color;
}

technique Technique1
{
	pass DefaultPass
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}