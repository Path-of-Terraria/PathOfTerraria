sampler uImage0 : register(s0);
float uTime;
float uPulse;
float uShimmer;
float3 uTint;
static const float ShimmerFrequencyX = 10.0;
static const float ShimmerFrequencyY = 6.0;
static const float TwoPi = 6.28318;
static const float ShimmerConcentration = 6.0;
static const float3 ShimmerAccentColor = float3(0.35, 0.62, 1.0);
static const float PulseIntensity = 0.12;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float4 baseColor = tex2D(uImage0, coords);
	float shimmerWave = 0.5 + 0.5 * sin((coords.x * ShimmerFrequencyX + coords.y * ShimmerFrequencyY + uTime) * TwoPi);
	float shimmer = pow(shimmerWave, ShimmerConcentration) * uShimmer;
	float pulse = uPulse * PulseIntensity;

	float3 color = baseColor.rgb * uTint;
	color += ShimmerAccentColor * shimmer;
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
