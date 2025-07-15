float2 scroll;
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

texture noiseTexture;

sampler2D sampler1 = sampler_state
{
	texture = <noiseTexture>;
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
	float2 uv = input.TextureCoordinates;
	float4 noiseValue = tex2D(sampler1, uv);
	float2 offset = noiseValue.xy * (1 - noiseValue.b);
	return tex2D(sampler0, uv + tex2D(sampler1, uv).xy - float2(0.5, 0.5) + scroll) * (1 - (distance(uv, float2(0.5, 0.5)) * 2));
}

technique Technique1
{
	pass DefaultPass
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}