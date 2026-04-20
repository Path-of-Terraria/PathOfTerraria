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

float2 canvasToScreenScale(float2 customCanvasScale)
{
	float2 scaledCanvas = customCanvasScale * additionalScale;
	return floor(screenRes / scaledCanvas + scaledCanvas);

}

float2 pixelsAmount(float customPixelSize, float2 customCanvasScale)
{
	return float2(customPixelSize,customPixelSize);
	//return floor(canvasToScreenScale(customCanvasScale) / customPixelSize);
}

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
float3 Palette(float2 t)
{
	return tex2D(uImage1, float2(t.x, 0)).rgb;
}

//pixelSize: the amount of pixels in the canvas

float4 PixelShaderFunction(VertexShaderOutput output) : COLOR0
{
	float oneLevel = 1 / 8.;
	float2 uv = output.TextureCoordinates;
	float2 pixelSizeBasedOnCanvasSize = pixelsAmount(pixelSize, canvasSize); // idk what did i make lol, but it works!
	uv = floor(uv * pixelSizeBasedOnCanvasSize) / pixelSizeBasedOnCanvasSize;
	uv = Rotate(uv, rotation, float2(.5, .5));
	float4 portal = tex2D(uImage0, uv);
	portal.rgba = floor(portal.rgba * 8) / 8 - oneLevel;
	portal.rgb = Palette(lerp(1, 0, length(portal.rgb) * 1));
	portal.a *= length(portal.rgb);
	
	//fix flickering pixels bug pleasse

	//dither texturing
	float2 ditherUV = output.TextureCoordinates.xy;
	float ditherTexture = tex2D(uImage2, ditherUV * (pixelSize.xx / 256));
	
	// get previous level palette 
	float4 portalPrevColor = tex2D(uImage0, uv);
	portalPrevColor.rgba = saturate(floor(portalPrevColor.rgba * 8) / 8) - oneLevel * 2 * ditherTexture;
	portalPrevColor.rgb = Palette(lerp(1, 0, length(portalPrevColor.rgb)));
	
	// put everything togather
	float4 ditherPortal = lerp(portal, (portalPrevColor), ditherTexture);
	
	//flash
	ditherPortal = lerp(ditherPortal, portal.a, data.y);
	
	//alpha based on color
	ditherPortal.a = length(ditherPortal.rgb);
	
	
	return ditherPortal;
}

technique t0
{
	pass P0
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
		VertexShader = compile vs_3_0 VertexShaderFunction();
	}
};
