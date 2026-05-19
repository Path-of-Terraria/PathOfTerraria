sampler uImage0 : register(s0);
float uTime;
float uPulse;
float uShimmer;
float3 uTint;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float4 baseColor = tex2D(uImage0, coords);
	float shimmerWave = 0.5 + 0.5 * sin((coords.x * 10.0 + coords.y * 6.0 + uTime) * 6.28318);
	float shimmer = pow(shimmerWave, 6.0) * uShimmer;
	float pulse = uPulse * 0.12;

	float3 color = baseColor.rgb * uTint;
	color += float3(0.35, 0.62, 1.0) * shimmer;
	color += pulse;

	return float4(saturate(color), baseColor.a);
}

technique Technique1
{
	pass PassiveTreeConnectorPass
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
