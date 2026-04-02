float2 scroll : register(c0);
float2 uvScale : register(c1);

texture sampleTexture : register(t0);

sampler2D tex0 = sampler_state
{
	texture = <sampleTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = wrap;
	AddressV = wrap;
};

texture mask;

sampler2D tex1 = sampler_state
{
	texture = <mask>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = wrap;
	AddressV = wrap;
};


float4 PixelShaderFunction(float2 uv : TEXCOORD) : COLOR0
{
	float4 sample = tex2D(tex0, uv * uvScale + scroll);
	float4 noise = tex2D(tex1, uv);
	
	float4 color = sample * noise.a;
	return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};