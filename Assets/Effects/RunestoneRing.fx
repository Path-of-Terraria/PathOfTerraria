matrix uWorldViewProjection;
float4 baseColor;
float width;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float invlerp(float from, float to, float value)
{
	return clamp((value - from) / (to - from), 0.0, 1.0);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 uv = input.TextureCoordinates;
	float dist = distance(uv, float2(0.5, 0.5));
	
	if (dist > 0.4 && dist < 0.4 + width)
	{
		return baseColor * (1 - ((abs(dist - 0.425)) / 0.025));
	}
	
	return float4(0, 0, 0, 0);
}

technique Technique1
{
    pass TreeShadowCastPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}