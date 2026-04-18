float blackCircle;
float4 data;
float uTime;
float rotation;
float2 positionsOffset;
float canvasSize;
float2 worldSize;
matrix transform;
float pixelSize;
float additionalScale;

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
	float4 Color : COLOR0;
	float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput) 0;
	float vertexPixelSize = pixelSize / length(canvasSize / additionalScale) / pixelSize;
	input.Position.xyz = ceil(input.Position.xyz / vertexPixelSize) * vertexPixelSize;
	
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

float4 PixelShaderFunction(VertexShaderOutput output) : COLOR0
{
	float2 uv = output.TextureCoordinates;
	float pixelSizeBasedOnCanvasSize = (pixelSize / (length(canvasSize / additionalScale) / pixelSize)); // idk what did i make lol, but it works!
	uv = floor(uv * pixelSizeBasedOnCanvasSize) / pixelSizeBasedOnCanvasSize;
	uv = Rotate(uv, rotation, float2(.5, .5));
	float4 portal = tex2D(uImage0, uv);
	portal.rgb = floor(portal.rgb * 8) / 8;
	portal.rgb = Palette(lerp(1, 0, length(portal.rgba) * 1.25f));

	//dither texturing
	pixelSizeBasedOnCanvasSize = (pixelSize / (length(8 * (canvasSize / additionalScale)))); // idk what did i make lol, but it works!

	float2 ditherUV = floor(output.TextureCoordinates * pixelSizeBasedOnCanvasSize) / (pixelSizeBasedOnCanvasSize);
	float4 ditherTexture = tex2D(uImage2, ditherUV) + float4(portal.rgb,1);
	float4 ditherPortal = portal * length(portal.a / ditherTexture);
	ditherPortal = floor(ditherPortal * 16) / 16;
	
	//flash
	ditherPortal = lerp(ditherPortal, portal.a, data.y);
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
