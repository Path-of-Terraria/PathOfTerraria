sampler uImage0 : register(s0);
sampler uImage1 : register(s1); 
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect; 
float2 uZoom;

float outerRadius;
float innerRadius;
float strength;
float curvature;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float2 curve = pow(abs(coords * 2.0 - 1.0), 1.0 / curvature);
    float edge = pow(length(curve), curvature);
    float vignette = 1.0 - strength * smoothstep(innerRadius, outerRadius, edge);
    
    color.rgb *= vignette;
    
    return color * uOpacity;
}

technique Technique1
{
    pass VignettePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};