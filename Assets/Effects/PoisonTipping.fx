float2 target;
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
	float2 uv = input.TextureCoordinates;
	float4 color = tex2D(sampler0, uv);
	color = lerp(color, float4(0.6, 1, 0.2, 0) * color.a, distance(uv, target) * 0.3);
	return color;
}

technique Technique1
{
	pass DefaultPass
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}