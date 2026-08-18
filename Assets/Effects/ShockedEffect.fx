sampler uImage0 : register(s0);

float uTime;
float uIntensity;
float uSeed;
float2 uBodyScale;
float3 uColor;

float Hash(float value)
{
	return frac(sin(value * 91.3458) * 47453.5453);
}

// Signed angular difference, wrapped to -pi..pi, so an arc that straddles the
// atan2 seam behind the entity doesn't get cut in half.
float AngleDelta(float a, float b)
{
	float d = a - b;
	return atan2(sin(d), cos(d));
}

// One arc of energy: a short angular span sitting just off the body, riding a radial
// path that wanders with layered sines so it crackles instead of tracing the hitbox.
// Each arc lives for one cycle, then respawns somewhere else around the entity.
float Arc(float angle, float radius, float index)
{
	float cycle = uTime * 1.45 + Hash(index * 37.0 + uSeed) * 10.0;
	float slot = floor(cycle);
	float t = frac(cycle);

	float r1 = Hash(slot * 13.0 + index * 71.0 + uSeed * 3.0);
	float r2 = Hash(slot * 29.0 + index * 17.0 + uSeed * 7.0);

	// Where it sits, how far it sweeps, and a slow drift across its lifetime.
	float centre = r1 * 6.2831853 + (r2 - 0.5) * t * 1.4;
	float halfWidth = 0.30 + r2 * 0.45;

	float d = AngleDelta(angle, centre);

	// Soft ends so the arc tapers out instead of stopping dead.
	float span = 1.0 - smoothstep(halfWidth * 0.5, halfWidth, abs(d));

	// Three sines at rising frequencies and opposing drift speeds: the slow one bends the
	// arc away from the silhouette, the middle one crackles, the fast one frays the edge.
	float wander = sin(d * 7.0 + uTime * 8.0 + r1 * 25.0) * 0.05
				 + sin(d * 15.0 - uTime * 13.0 + r2 * 40.0) * 0.03
				 + sin(d * 29.0 + uTime * 19.0 + r1 * 55.0) * 0.012;

	float path = 1.05 + r1 * 0.09 + wander;
	float dist = abs(radius - path);

	float core = 1.0 - smoothstep(0.006, 0.05, dist);
	float glow = 1.0 - smoothstep(0.02, 0.20, dist);

	// Snap on, hold, then decay - a discharge, not a fade in and out.
	float fade = smoothstep(0.0, 0.10, t) * (1.0 - smoothstep(0.45, 1.0, t));

	return (core * 0.85 + glow * 0.22) * span * fade;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float2 centered = coords * 2.0 - 1.0;
	float2 bodyPoint = centered / max(uBodyScale, float2(0.05, 0.05));
	float radius = length(bodyPoint);
	float angle = atan2(bodyPoint.y, bodyPoint.x);

	// Three arcs on independent cycles, so there is usually one or two present and
	// occasionally a brief overlap, without ever closing into a ring.
	float energy = Arc(angle, radius, 1.0)
				 + Arc(angle, radius, 2.0)
				 + Arc(angle, radius, 3.0);

	float alpha = saturate(energy) * uIntensity;
	alpha *= 1.0 - smoothstep(1.45, 1.68, radius);

	return float4(uColor, alpha) * sampleColor;
}

technique Technique1
{
	pass P0
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
};
