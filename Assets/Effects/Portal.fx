float4 data;
float uTime;
float rotation;
float2 pixelCalculationFormula;
float canvasSize;
matrix transform;
float pixelSize;
float additionalScale;
float2 screenRes;
texture sampleTexture;
float paletteColorsAmount;
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
	float4 Color : COLOR0;
	float3 TextureCoordinates : TEXCOORD0;
};
// pixelSize / length(canvasSize / additionalScale) / pixelSize; //the formula
VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput) 0;
	float2 vertexPixelSize = pixelSize;
	input.Position.xy = floor(input.Position.xy * vertexPixelSize) / vertexPixelSize;
	
	float4 pos = mul(input.Position, transform);
	
	output.Position = pos;
    
	output.Color = input.Color;
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}

float2 Rotate(float2 uv, float angle, float2 pivot)
{
	float2x2 rotationMatrix = float2x2(cos(angle), sin(angle), -sin(angle), cos(angle));
	uv -= pivot;
	float2 r = mul(rotationMatrix, uv);
	r += pivot;
	
	return r;
    
}
float2 SinBetween(float2 a, float2 b, float2 v)
{
	float h = (b - a) / 2.;
	return a + h + sin
    (v) * h;
}

// returns the color
float3 palette(float index)
{
	return tex2D(uImage1, float2(index, 0)).rgb;
}

float toIndex(float v)
{
	return floor(v * paletteColorsAmount) / paletteColorsAmount;

}

float3 Palette(float4 v)
{
	return palette(toIndex(saturate(length(v))));
}


//pixelSize: the amount of pixels in the canvas

float4 PixelShaderFunction(VertexShaderOutput output) : COLOR0
{
	float oneLevel = 1 / paletteColorsAmount; // the indexer of the palette texture
	float2 uv = output.TextureCoordinates;
	float2 pixelSizeBasedOnCanvasSize = pixelSize; // pixel size
	uv = floor(uv * pixelSizeBasedOnCanvasSize) / pixelSizeBasedOnCanvasSize; // pixel effect uv
	float4 col = 0; //  return value
	float4 invertedCol = 0; //  return value
	
	// draw the portal. then draw the dither texture which its pattern isnt visible when the alpha isnt equal to (index % 2 == 1)
	float4 portalBase = tex2D(uImage0, Rotate(uv, rotation, float2(.5, .5)));
	float4 ditherPatternTexture = tex2D(uImage2, output.TextureCoordinates.xy * (pixelSizeBasedOnCanvasSize / 256));
	float ditherPattern_NonInverted = ditherPatternTexture.r;
	float ditherPattern_Inverted = step(ditherPatternTexture.r, 1);
	
	float3 currentColor = palette(toIndex(portalBase.r) - oneLevel); // add extra level to increase the color brightness
	float3 prevColor = palette(saturate(toIndex(portalBase.r) - oneLevel * 2)); // same with this one
	
	// setup dither Textures to be added to col
	
	float ditherAlpha = lerp(0, 1, toIndex(portalBase.r));
	
	col += float4(currentColor * ditherPattern_Inverted,ditherPattern_NonInverted);
	float alphaCut = step(oneLevel * .05, toIndex(ditherAlpha));
	float3 invertedDitherCol = lerp(portalBase.r, currentColor, alphaCut); //palette(saturate(toIndex(portalBase.r) - (oneLevel * (toIndex(ditherAlpha) % 2))));
	invertedCol += float4(invertedDitherCol * ditherPattern_Inverted * alphaCut, alphaCut);
	
	//clean ups (remove flickering pixels bug, remove black pixels in both outside and inside the shader effect)
	float4 finalCol = (col + invertedCol) * step(0, length(uv * 2 - 1) - data.w);
	finalCol *= step(length(uv * 2 - 1) - data.z,0);
	
	float4 blackHole = 0;
	blackHole.a = smoothstep(.5, length(uv * 2 - 1) * 1.2 - data.w, data.w) * 2;
	//finalCol += lerp(blackHole, finalCol, alphaCut);
	finalCol = lerp(finalCol, blackHole.a, data.y);

	return finalCol;
}

technique t0
{
	pass P0
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
		VertexShader = compile vs_3_0 VertexShaderFunction();
	}
};
