float4 data;
float uTime;
float rotation;
matrix transform;
float pixelSize;
float paletteColorsAmount;
float ditherSize;

texture sampleTexture;
sampler2D uImage0 : register(s0) = sampler_state
{
	texture = <sampleTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler2D uImage1 : register(s1) = sampler_state
{
	texture = <sampleTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture bayerTexture;
sampler2D uImage2 : register(s2) = sampler_state
{
	texture = <bayerTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	input.Position.xy = floor(input.Position.xy * pixelSize) / pixelSize;
	output.Position = mul(input.Position, transform);
	output.TextureCoordinates = input.TextureCoordinates;
	return output;
}

float2 Rotate(float2 uv, float angle, float2 pivot)
{
	float2x2 rotationMatrix = float2x2(cos(angle), sin(angle), -sin(angle), cos(angle));
	uv -= pivot;
	uv = mul(rotationMatrix, uv);
	return uv + pivot;
}

float3 PaletteColor(float index)
{
	return tex2D(uImage1, float2(index, 0.0f)).rgb;
}

float PaletteIndex(float value)
{
	return floor(value * paletteColorsAmount) / paletteColorsAmount;
}

float4 PixelShaderFunction(VertexShaderOutput output) : COLOR0
{
	float oneLevel = 1.0f / paletteColorsAmount;
	float2 uv = output.TextureCoordinates.xy;
	uv = ceil(uv * pixelSize) / pixelSize;
	uv += sin(uv + (uTime * 0.05f)) * 0.01f;

	float4 portalBase = tex2D(uImage0, Rotate(uv, rotation, float2(0.5f, 0.5f)));
	float ditherPattern = tex2D(uImage2, uv * ditherSize).r;
	float invertedDitherPattern = 1.0f - ditherPattern;
	float3 currentColor = PaletteColor(saturate(PaletteIndex(portalBase.r) - oneLevel));
	float ditherAlpha = 1.0f - PaletteIndex(portalBase.r);

	float4 color = float4(currentColor * invertedDitherPattern, ditherPattern);
	float alphaCut = smoothstep(0.1f, 0.995f, PaletteIndex(ditherAlpha));
	float3 invertedDitherColor = lerp(portalBase.rrr, currentColor, alphaCut);
	color += float4(invertedDitherColor * invertedDitherPattern * alphaCut, alphaCut);

	color *= step(length((uv * 2.0f) - 1.0f) - data.z, 0.0f);
	float flash = step(0.1f, color.a);
	return lerp(color, float4(flash, flash, flash, flash), data.y);
}

technique t0
{
	pass P0
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
		VertexShader = compile vs_3_0 VertexShaderFunction();
	}
};
