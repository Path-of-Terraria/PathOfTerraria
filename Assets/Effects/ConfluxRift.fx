float3 u_mouse;
float u_time;
float3 primary;
float3 primaryScaling;
float3 secondary;

float progress;
float timeManual;

texture sampleTexture : register(ps, s0);
sampler2D u_tex0 = sampler_state { Texture = sampleTexture; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture _PaletteTex : register(ps, s1);
sampler2D u_tex2 = sampler_state { Texture = _PaletteTex; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = Clamp; AddressV = Clamp; };

texture _PNoiseTex : register(ps, s2);
sampler2D u_tex3 = sampler_state { Texture = _PNoiseTex; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = Wrap; AddressV = Wrap; };

texture _DNoiseTex : register(ps, s3);
sampler2D u_tex4 = sampler_state { Texture = _DNoiseTex; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = Wrap; AddressV = Wrap; };

float inverseLerp(float a, float b, float val)
{
	float dist = b - a;
	float progress = val - a;

	return (progress / dist);
}

float2 makeCoordsRadial(float2 coords)
{
	float x = inverseLerp(-3.14, 3.14, atan2(coords.x, coords.y));
	float y = length(coords);
	return float2(x, y);
}

float2 makeCoordsRadialCenter(float2 coords)
{
	return makeCoordsRadial(coords - float2(0.5, 0.5));
}

float clamp01(float inp)
{
	return clamp(inp, 0, 1);
}

float fract(float inp)
{
	return inp % 1.0;
}

float4 PixelShaderFunction(float2 startUV : TEXCOORD) : COLOR0
{
	// =====================================================
	float ballProgress = clamp01(inverseLerp(0.0, 0.2, progress));

	float expandProgress = clamp01(inverseLerp(0.3, 0.5, progress));
	expandProgress = pow(expandProgress, 0.65);

	float ringProgress = clamp01(inverseLerp(0.3, 0.5, progress));
	ringProgress = pow(ringProgress, 2.5);

	float smallRiftXNoiseScale = 0.4; // Increase for more chaotic X noise on the rift before expansion
	float largeRiftXNoiseScale = 0.5; // Increase for more chaotic X noise on the rift after expansion

	float smallRiftYNoiseScale = 0.6; // Increase for more chaotic Y noise on the rift before expansion
	float largeRiftYNoiseScale = 0.6; // Increase for more chaotic Y noise on the rift after expansion

	float riftXScrollSpeed = 0.1; // Increase for the rift's D noise to scroll faster
	float dNoiseOffset = -0.5;

	float anchorCenterX = 0.5;
	float anchorCenterY = 0.5;

	float anchorSwayBeforeExpansion = 0.3; // Increase for the rift to sway more on the X axis before expansion
	float anchorSwayAfterExpansion = 0.6; // Increase for the rift to sway more on the X axis after expansion

	float anchorCenterPowBefore = 0.45; // Increase to more rigidly "pinch" at the center of the rift before expansion
	float anchorCenterPowAfter = 0.9; // Increase to more rigidly "pinch" at the center of the rift after expansion

	float thicknessNoiseYScrollSpeed = 0.21; // Increase for faster scroll on X noise that determiens thickness
	float thicknessNoiseYScaleBefore = 2.0; // Increase for more chaotic thickness variation on the Y axis before expansion
	float thicknessNoiseYScaleAfter = 1.1; // Increase for more chaotic thickness variation on the Y axis after expansion

	float thicknessValPow = 1.0; // Increase for more thickness/less variance

	float minThicknessPow = 30.0; // Increase for a thinner maximum thickness
	float maxThicknessPow = lerp(200.0, 80.0, expandProgress); // Increase for a thinner minimum thickness

	float thicknessPowBeforeExpansion = 1.0;
	float thickenssPowAfterExpansion = 0.4;

	float thicknessTotalPow = lerp(2.0, 2.0, expandProgress);

	float riftHeightInverseBefore = 7.0;
	float riftHeightInverseAfter = 2.05;

	float riftHeightPow = 2.0;

	float radialCoordsXMult = 1.0;
	float radialCoordsYMult = 3.0;

	float numRings = 2.0;

	float ringThickness = 1.0;

	float ringPowStart = 1.0;
	float ringPowEnd = 1.5;

	float ringDistanceStart = 0.3;
	float ringDistanceEnd = 2.5;

	float ringThicknessStart = 40;
	float ringThicknessEnd = 2.0;

	float ringEasingPow = 0.1;
	float ringSpinSpeed = 0.5;

	float ballXScaleStart = 40.0; // Increase to make ball smaller on X scale at start of ball sequence
	float ballXScaleEnd = 11.0; // Increase to make ball smaller on X scale at end of ball sequence
	float ballXScaleEasing = 0.2;

	float ballYScaleStart = 40.0; // Increase to make ball smaller on Y scale at start of ball sequence
	float ballYScaleEnd = 11.0; // Increase to make ball smaller on Y scale at end of ball sequence
	float ballYScaleEasing = 0.33;

	float ballYScaleBeforeExpand = 1.0;
	float ballYScaleAfterExpand = 0.2;

	float ballRoundness = 4.0; // Increase for a smoother, rounder ball
	float ballSize = 0.15; // Increase for a bigger ball
	float ballSizePow = 2.0;

	float ballValPow = 1.85 * lerp(1.0, 5.0, pow(expandProgress, 2.0));

	float paletteBallMult = 1.4;
	float paletteBallCutoff = 1.0000;

	// ========================================================
	float2 uv = startUV;
	uv = floor(uv * 256.0) / 256.0;
	float2 dNoiseCoords = float2(uv.x * lerp(smallRiftXNoiseScale, largeRiftXNoiseScale, step(0.0001, expandProgress)), uv.y * lerp(smallRiftYNoiseScale, largeRiftYNoiseScale, expandProgress));
	dNoiseCoords.x += (min(progress, 0.65) * 6) * riftXScrollSpeed;
	dNoiseCoords.x += timeManual * 0.1;
	float dNoiseVal = tex2D(u_tex4, dNoiseCoords).r;
	dNoiseVal += dNoiseOffset;

	float anchorX = anchorCenterX + (lerp(anchorSwayBeforeExpansion, anchorSwayAfterExpansion, expandProgress) * dNoiseVal * pow(abs(uv.y - anchorCenterY), lerp(anchorCenterPowBefore, anchorCenterPowAfter, step(0.0001, expandProgress))));
	float centerDistX = abs(anchorX - uv.x);

	float2 noiseCoordsTwo = float2((timeManual * thicknessNoiseYScrollSpeed), uv.y * lerp(thicknessNoiseYScaleBefore, thicknessNoiseYScaleAfter, expandProgress));
	float noiseVal2 = tex2D(u_tex3, noiseCoordsTwo).r;
	noiseVal2 = pow(noiseVal2, thicknessValPow);
	centerDistX = pow(centerDistX * lerp(minThicknessPow, maxThicknessPow, noiseVal2) * lerp(thicknessPowBeforeExpansion, thickenssPowAfterExpansion, expandProgress), thicknessTotalPow);
	float paletteX = centerDistX;

	float centerDistY = abs(uv.y - 0.5) * lerp(riftHeightInverseBefore, riftHeightInverseAfter, expandProgress);
	centerDistY = pow(centerDistY, riftHeightPow);
	paletteX += centerDistY;
	paletteX = lerp(paletteX, 1.0, (clamp01(ballProgress * 2.0) - expandProgress));
	paletteX = clamp01(paletteX);

	float2 radialCoords = makeCoordsRadialCenter(uv);
	radialCoords.x *= radialCoordsXMult;
	radialCoords.y *= radialCoordsYMult;

	radialCoords.y -= timeManual * ringSpinSpeed;

	float ballVal = tex2D(u_tex4, radialCoords).r + 0.03;

	float2 ballLength = uv - float2(lerp(0.5, anchorX, expandProgress), 0.5);
	ballLength.x *= lerp(ballXScaleStart, ballXScaleEnd, pow(ballProgress - expandProgress, ballXScaleEasing));
	ballLength.y *= lerp(ballYScaleStart, ballYScaleEnd, pow(ballProgress, ballYScaleEasing));
	ballLength.y *= lerp(ballYScaleBeforeExpand, ballYScaleAfterExpand, expandProgress);
	ballVal *= 0.1 - abs(pow(ballRoundness * (ballSize - length(ballLength)), ballSizePow));

	if (ballVal < -0.99)
	{
		ballVal = -0.99;
	}
	ballVal = inverseLerp(-1, 0.3, ballVal);
	ballVal = pow(ballVal, ballValPow);
	//paletteX -= ballVal * ballProgress * step(paletteBallCutoff,paletteX) * paletteBallMult;
	paletteX = lerp(paletteX, paletteX - (ballVal * ballProgress * paletteBallMult), 1.0 - step(paletteX, paletteX - (ballVal * ballProgress * paletteBallMult)));
	//paletteX -= ringVal * ballProgress;

	float2 portalCoords = makeCoordsRadial(float2((uv.x - 0.5) * lerp(7.0, 3.5, expandProgress), (uv.y - 0.5) * 1.6));

	float2 bumpCoords = float2((portalCoords.x * 1.0) + (timeManual * 0.1), (portalCoords.y) - (timeManual * 0.15));
	float portalBumps = tex2D(u_tex4, bumpCoords).r;
	portalCoords.y += 0.32 * portalBumps;
	portalCoords.x *= 0.75;
	float portalDist = portalCoords.y;
	portalCoords.y *= 0.5;
	portalCoords.y += timeManual * 0.15;
	float portalVal = tex2D(u_tex3, portalCoords).r + 0.1;

	portalVal *= pow(inverseLerp(0.2 * pow(expandProgress, 1.7), 0.4 * pow(expandProgress, 1.4), portalDist), 3);
	paletteX = min(paletteX, portalVal);
	float2 paletteCoords = float2(paletteX, tex2D(u_tex0, uv).r * 0.1);
	float4 ret = tex2D(u_tex2, paletteCoords);

	if (length(ret.rgb) == 0.0)
	{
		return float4(0, 0, 0, 0);
	}

	return ret;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
};