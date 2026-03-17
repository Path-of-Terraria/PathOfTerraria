float2 topLeft : register(c0);
float2 size : register(c1);

float2 sampleSize : register(c2);
float2 noiseSize : register(c3);

texture sampleTexture : register(t0);
sampler2D tex0 = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture noiseTexture;
sampler2D tex1 = sampler_state { texture = <noiseTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };


float4 PixelShaderFunction(float2 uv : TEXCOORD) : COLOR0
{
	float4 sample = tex2D(tex0, uv);
	float4 noise = tex2D(tex1, uv / sampleSize);
	float4 color = sample * noise * noise.b;
	float dist = distance(uv, float2(0.5, 0.5));
	
	color *= saturate(0.5 - dist);
	
	//if (sample.b == 0 && sample.r != 0)
	//{
	//	return float4(0, 0, 0, 0);
	//}
	
	return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};