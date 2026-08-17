float uTime;
float3 uPrimary;
float3 uSecondary;
float uMaterial;
float uGrade;
float uSeed;

texture sampleTexture;
sampler2D sampler0 = sampler_state
{
	texture = <sampleTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float stroke(float value, float width)
{
	return 1.0 - smoothstep(width, width * 2.0, abs(value));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 uv = input.TextureCoordinates;
	float2 p = uv - 0.5;

	float radius = length(p);
	float angle = atan2(p.y, p.x);
	float time = uTime + uSeed * 9.0;
	float noiseA = tex2D(sampler0, uv * 2.2 + float2(time * 0.025, -time * 0.04)).r;
	float noiseB = tex2D(sampler0, uv * 4.1 + float2(-time * 0.035, time * 0.02)).g;

	float inside = 1.0 - smoothstep(0.46, 0.5, radius);
	float rim = stroke(radius - 0.435, 0.012);
	float innerRim = stroke(radius - 0.355, 0.006) * 0.55;
	float rotatingBars = stroke(sin(angle * 6.0 + time * 0.8), 0.16) * smoothstep(0.27, 0.4, radius);
	float runicBand = stroke(sin(angle * 12.0 - time * 0.55) + 0.55, 0.12) * stroke(radius - 0.39, 0.025);
	float gradeBand = stroke(radius - (0.325 - uGrade * 0.012), 0.005) * uGrade * 0.28;

	float materialPattern = 0.0;
	if (uMaterial < 0.5) // Vigor: pulsing veins
	{
		materialPattern = stroke(sin(angle * 3.0 + radius * 24.0 - time * 2.2), 0.16) * (0.4 + noiseA);
	}
	else if (uMaterial < 1.5) // Bastion: interlocking masonry
	{
		float blocks = min(abs(sin(p.x * 31.0)), abs(sin((p.y + floor(p.x * 8.0) * 0.08) * 27.0)));
		materialPattern = stroke(blocks, 0.11);
	}
	else if (uMaterial < 2.5) // Embers: rising flame tongues
	{
		materialPattern = stroke(sin(p.x * 25.0 + noiseA * 5.0 + p.y * 11.0 - time * 4.0), 0.17) * smoothstep(0.48, -0.32, p.y);
	}
	else if (uMaterial < 3.5) // Rime: crystalline spokes
	{
		materialPattern = pow(abs(cos(angle * 6.0)), 22.0) + stroke(sin(radius * 34.0), 0.1) * 0.5;
	}
	else if (uMaterial < 4.5) // Tempests: branching arcs
	{
		materialPattern = stroke(sin(angle * 4.0 + radius * 38.0 + noiseB * 5.0 - time * 7.0), 0.13);
	}
	else if (uMaterial < 5.5) // Void: inward spiral
	{
		materialPattern = stroke(sin(angle * 5.0 + radius * 31.0 + time * 1.8), 0.16) * (0.45 + noiseB);
	}
	else if (uMaterial < 6.5) // Might: cleaving chevrons
	{
		materialPattern = stroke(frac((abs(p.x) + p.y - time * 0.08) * 11.0) - 0.5, 0.08);
	}
	else if (uMaterial < 7.5) // Precision: crosshair and measured rings
	{
		materialPattern = max(stroke(p.x, 0.012), stroke(p.y, 0.012));
		materialPattern = max(materialPattern, stroke(radius - 0.23, 0.007));
	}
	else if (uMaterial < 8.5) // Haste: rushing diagonal streaks
	{
		materialPattern = stroke(sin((p.x * 1.8 + p.y) * 35.0 - time * 8.0), 0.11) * (0.55 + noiseA);
	}
	else // Spirit: drifting wisps
	{
		materialPattern = stroke(sin(p.x * 17.0 + sin(p.y * 12.0 + time) * 2.0 + time * 2.0), 0.18) * (0.4 + noiseB);
	}

	materialPattern *= inside * smoothstep(0.06, 0.34, radius) * 0.34;
	float shimmer = 0.78 + 0.22 * sin(time * 3.0 + angle * 2.0 + noiseA * 5.0);
	float intensity = (rim + innerRim + gradeBand + rotatingBars * 0.28 + runicBand * 0.6 + materialPattern) * shimmer;
	intensity += inside * (noiseA * noiseB) * 0.09;
	intensity *= inside * (0.78 + uGrade * 0.18);

	float colorMix = saturate(materialPattern * 2.4 + runicBand + noiseB * 0.25);
	float3 color = lerp(uPrimary, uSecondary, colorMix);
	return float4(color * intensity, saturate(intensity));
}

technique Technique1
{
	pass DefaultPass
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
