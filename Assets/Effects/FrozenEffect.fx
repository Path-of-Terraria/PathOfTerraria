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
	float scroll = (input.TextureCoordinates.x + input.TextureCoordinates.y + scroller) % 1;
	
	// This adds in the little 'glint' effect
	if (scroll < 0.02 || (scroll > 0.49 && scroll < 0.51))
	{
		lum = 1;
	}
	
	// Lerp the color based on the luminosity of the input color; basically, remap the palette to an icy blue ramp
	color = lerp(lerp(float4(0.06, 0.1, 0.6, 1), float4(0.1, 0.18, 0.8, 1), lum), lerp(float4(0.2, 0.3, 0.8, 1), float4(0.85, 0.9, 1, 1), lum), lum);
	
	// But also keep the alpha consistency so we don't get random opaque blocks
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