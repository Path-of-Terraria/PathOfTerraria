sampler uImage0 : register(s0); // base/noise texture sampled for shimmer

float uTime;
float uIntensity; // 0..1 — current shield fraction
float3 uColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords * 2.0 - 1.0;
    float dist = length(centered);

    if (dist > 1.0)
    {
        return float4(0, 0, 0, 0);
    }

    // Soft outer halo: brightest near the unit circle's edge, fades quickly past it
    float ring = smoothstep(0.55, 0.85, dist) * (1.0 - smoothstep(0.85, 1.0, dist));

    // Faint inner fill so the shape inside the ring is also tinted (subtle)
    float innerFill = (1.0 - smoothstep(0.0, 0.95, dist)) * 0.18;

    // Animated noise sample for shimmer
    float2 noiseUV = coords + float2(uTime * 0.03, uTime * -0.025);
    float noise = tex2D(uImage0, noiseUV).r;

    // Slow breathing pulse so the shield feels alive
    float pulse = 0.85 + 0.15 * sin(uTime * 2.0);

    // Collapse ring inward as shield drops, so a near-broken shield reads as "thin/weak"
    float reach = lerp(0.55, 1.0, uIntensity);
    if (dist > reach)
    {
        return float4(0, 0, 0, 0);
    }

    float alpha = (ring + innerFill) * (0.55 + 0.45 * noise) * pulse * uIntensity;

    return float4(uColor, 1.0) * alpha;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};
