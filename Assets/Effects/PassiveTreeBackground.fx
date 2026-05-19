float uTime;
float uOpacity;
float uStarIntensity;
float uNebulaIntensity;
float3 uBaseTopColor;
float3 uBaseBottomColor;

float hash21(float2 p)
{
	p = frac(p * float2(123.34, 456.21));
	p += dot(p, p + 45.32);
	return frac(p.x * p.y);
}

float starField(float2 uv, float scale, float threshold, float twinkleSpeed)
{
	float2 grid = uv * scale;
	float2 id = floor(grid);
	float2 gv = frac(grid) - 0.5;

	float rnd = hash21(id);
	float twinkle = 1;
	float starMask = smoothstep(threshold, 1.0, rnd);

	float falloff = smoothstep(0.07, 0.0, length(gv));
	return starMask * falloff * twinkle;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float2 uv = coords;

	float3 baseColor = lerp(uBaseTopColor, uBaseBottomColor, uv.y);

	float2 driftUv = uv;
	driftUv.x += uTime * 0.03;
	driftUv.y -= uTime * 0.02;

	float nebula = 0.5 + 0.5 * sin((driftUv.x + driftUv.y) * 16.0 + sin(driftUv.y * 11.0 + uTime * 0.35) * 1.2);
	float3 nebulaColor = float3(0.08, 0.12, 0.20) * nebula * uNebulaIntensity;

	float starsNear = starField(uv + float2(uTime * 0.0005, 0), 90.0, 0.960, 0.3);
	float starsFar = starField(uv + float2(-uTime * 0.0002, uTime * 0.0005), 150.0, 0.99, 0.2);
	float stars = saturate(starsNear + starsFar * 0.7) * uStarIntensity;

	float3 color = baseColor + nebulaColor + stars;

	return float4(saturate(color), 1.0) * uOpacity;
}

technique Technique1
{
	pass PassiveTreeBackgroundPass
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}