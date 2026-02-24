sampler uImage0 : register(s0); // Noise texture
sampler uImage1 : register(s1); // Water RT
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

float auraPixelSize : register(c0);

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

float2 bossPositions[6] = { float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0), float2(0, 0) };
float waterHeight = 1;
float waterOpacity = 1;
bool hasWater = false;

// Dithering, thanks Mirsario
float brightness(float3 color)
{
	return dot(color, float3(0.299, 0.587, 0.114));
}
float brightness(float4 color)
{
	return dot(color.rgb, float3(0.299, 0.587, 0.114));
}

float sqrLength(float3 vec)
{
	return (vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
}
float sqrDistance(float3 a, float3 b)
{
	return sqrLength(a - b);
}

const float4x4 ditherMatrix =
{
	+0.0, +8.0, +2.0, 10.0,
    12.0, +4.0, 14.0, +6.0,
    +3.0, 11.0, +1.0, +9.0,
    15.0, +7.0, 13.0, +5.0
};

float3 dither(float2 position, float3 srcColor, float stepValue, float3 colorA, float3 colorB)
{
	float srcToA = distance(srcColor, colorA);
	float srcToB = distance(srcColor, colorB);
	float aToB = distance(colorA, colorB);
	float checkValue = srcToA / aToB;
    
	int x = int(fmod(position.x, 4));
	int y = int(fmod(position.y, 4));
	float matrixValue = ditherMatrix[x + (y * 4)] / 16.0;

	return checkValue < matrixValue ? colorA : colorB;
}

float2 evenScreenDistance(float2 coords, int bossIndex)
{
	float2 boss = bossPositions[bossIndex];
	float2 tail = bossPositions[bossIndex + 3];
	
	return sqrt(pow(boss.x - coords.x, 2) + pow((boss.y - coords.y) * (uScreenResolution.y / uScreenResolution.x), 2));
}

float4 FilterMyShader(float2 coords : TEXCOORD0) : COLOR0
{
	//coords = round(coords * uScreenResolution * 0.5) / (0.5 * uScreenResolution);
	
	float4 color = tex2D(uImage0, coords);
	
	coords = round(coords / uZoom * uScreenResolution * 0.5) / (0.5 * uScreenResolution);
	coords -= lerp(float2(0, 0), float2(0.25, 0.25), sqrt(1 - uZoom)); // Tries to adjust shader effects to "zoom" properly, doesn't work atm
	float reduction = min(min(evenScreenDistance(coords, 0), evenScreenDistance(coords, 1)), evenScreenDistance(coords, 2));
	float range = auraPixelSize / uScreenResolution.x;
	
	if (hasWater)
	{
		range = 0;
	}
	
	if (reduction < range)
	{
		return color;
	}
	
	float effect = uIntensity;
	
	if (coords.y > waterHeight) // Fade out (water check)
	{
		float off = (coords.y - waterHeight);
		effect *= lerp(1 - off / 0.05f, 0, waterOpacity);
		effect = max(0, effect);
		
		if (off > 0.05f)
		{
			return color;
		}
	}
	
	if (reduction < range * 1.2f) // Fade out (distance check)
	{
		effect *= (reduction - range) / (range * 1.2f - range);
	}
	
	color = tex2D(uImage0, coords);
	
	// Smog pass
	float4 poison = tex2D(uImage1, frac(coords));
	poison = tex2D(uImage1, frac(coords + float2(uProgress + uDirection.x + poison.g * 0.05, uProgress * 0.25 + uDirection.y + poison.g * 0.05)));
	
	color = lerp(color, float4(0.55, 0.2, 0.8, 1), poison.r * effect);
	color.g -= effect * 0.3;
	
	return color;
	
	// Dithering/quantization pass (Doesn't work properly)
	//float value = clamp(brightness(color), 0.0, 1.0);
	//float valuesPerChannel = 10; //VALUES_PER_CHANNEL;
	
	//float3 darker = (floor(color * valuesPerChannel) + 0) / (valuesPerChannel);
	//float3 brighter = (floor(color * valuesPerChannel) + 1) / (valuesPerChannel);
	//float step01 = (value - (floor(value * valuesPerChannel) / (valuesPerChannel))) / (1.0 / valuesPerChannel);
	//float3 dithered = dither(coords, color.rgb, step01,	darker, brighter);

	//return float4(dithered, color.a);
}

technique Tech
{
	pass Pass0
	{
		PixelShader = compile ps_3_0 FilterMyShader();
	}
}