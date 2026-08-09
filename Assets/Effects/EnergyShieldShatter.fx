sampler uImage0 : register(s0); // noise texture for shard pattern

float uProgress; // 0..1 over the shatter lifetime
float3 uColor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords * 2.0 - 1.0;
    float dist = length(centered);

    if (dist > 1.0)
    {
        return float4(0, 0, 0, 0);
    }

    // Shockwave: a ring whose radius grows from 0 to 1 across the lifetime
    float ringRadius = uProgress;
    float ringWidth = 0.18 * (1.0 - uProgress * 0.5);
    float ringInner = ringRadius - ringWidth;
    float ringOuter = ringRadius + ringWidth * 0.4;
    float ring = smoothstep(ringInner, ringRadius, dist) * (1.0 - smoothstep(ringRadius, ringOuter, dist));

    // Polar-mapped noise so shards read as outward-streaking fragments rather than tiled blobs
    float angle = atan2(centered.y, centered.x) / 6.2831853 + 0.5;
    float2 polarUV = float2(angle * 3.0, dist - uProgress * 0.7);
    float noise = tex2D(uImage0, polarUV).r;
    float shards = step(0.5 - uProgress * 0.25, noise);

    // Bright inner flash that decays fast — sells the moment of impact
    float flash = (1.0 - smoothstep(0.0, 0.55, dist)) * (1.0 - uProgress) * (1.0 - uProgress);

    float alpha = (ring * shards + flash) * (1.0 - uProgress);

    return float4(uColor, 1.0) * alpha;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};
