#ifndef OSFOG_INCLUDE
#define OSFOG_INCLUDE

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile _ _SHADOWS_SOFT
#pragma multi_compile _ _BUTO_SELF_ATTENUATION_ENABLED
#pragma multi_compile _ _BUTO_HORIZON_SHADOWING_ENABLED
#pragma multi_compile _ _BUTO_ANALYTIC_FOG_ENABLED


half3 rayDirection;
half3 mainLightDirection;
half3 mainLightColor;

int _LightCountButo = 0;
half3 _LightPosButo[8];
half _LightIntensityButo[8];
half3 _LightColorButo[8];

int _VolumeCountButo = 0;
half4 _VolumeDataButo[8]; // xyz world pos, w radius
half _VolumeIntensityButo[8]; // Intensity scalar

int _MaximumSelfShadowingOctaves = 1;
half3 _WorldColor = half3(0, 0, 0);
int _FogMaskBlendMode = 0;

half InverseLerp(half a, half b, half v)
{
	return (v - a) / (b - a);
}

half Remap(half iMin, half iMax, half oMin, half oMax, half v)
{
	half t = InverseLerp(iMin, iMax, v);
	return saturate(lerp(oMin, oMax, t));
}

half CalculateHorizonFalloff(half3 RayPosition, half3 LightDirection)
{
	half h = max(RayPosition.y, 0);
	half r = 6371000.0; // Earth Radius (m)
	half a = r + h;
	half b = r / a;
	half c = acos(b);
	LightDirection = normalize(LightDirection);
	half angle = LightDirection.y * 1.57079632679;
	half d = angle - c;
	
	return smoothstep(0.0, radians(5.0), d);
}

half GetLightFalloff(half3 RayPosition, half3 LightPosition)
{
	half d = distance(RayPosition, LightPosition);
	return 1.0 / ((1.0 + (d * d)));
}

half3 GetAdditionalLightData(half3 RayPosition)
{
	half3 finalColor = half3(0, 0, 0);
	
	for (int lightIndex = 0; lightIndex < _LightCountButo; lightIndex++)
	{
		finalColor += _LightColorButo[lightIndex] * GetLightFalloff(RayPosition, _LightPosButo[lightIndex]) * _LightIntensityButo[lightIndex];
	}
	return finalColor;
}

half GetShadowAttenuation(half3 RayPos)
{
	#ifndef SHADERGRAPH_PREVIEW
	#if SHADOWS_SCREEN
	   half4 clipPos = TransformWorldToHClip(RayPos);
	   half4 shadowCoord = ComputeScreenPos(clipPos);
	#else
		half4 shadowCoord = TransformWorldToShadowCoord(RayPos);
	#endif
		return saturate(GetMainLight(shadowCoord).shadowAttenuation);
	#endif
}

float rand2dTo1d(float2 vec, float2 dotDir = float2(12.9898, 78.233))
{
	float random = dot(sin(vec.xy), dotDir);
	random = frac(sin(random) * 143758.5453);
	return random;
}

float2 rand2dTo2d(float2 vec, float2 seed = 4605)
{
	return float2(
		rand2dTo1d(vec + seed, float2(12.989, 78.233)),
		rand2dTo1d(vec + seed, float2(39.346, 11.135))
	);
}

half GetRandomHalf(half2 Seed)
{
	return saturate(frac(sin(dot(Seed + half2(642.86, 808.10), half2(12.9898, 78.233))) * 43758.5453));
}

half HenyeyGreenstein(half eccentricity)
{
	half cosAngle = dot(normalize(mainLightDirection), normalize(rayDirection));
	half e2 = eccentricity * eccentricity;
	return ((1.0 - e2) / pow(abs(1.0 + e2 - 2.0 * eccentricity * cosAngle), 1.5)) / 4.0 * 3.1416;
}

half GetFogDensityByHeight(half AttenuationBoundarySize, half BaseHeight, half3 RayPosition)
{
	half height = max(RayPosition.y - BaseHeight, 0.0);
	return exp(-height * 1.0 / max(AttenuationBoundarySize, 0.001));
}

half GetFogDensityByNoise(Texture3D NoiseTexture, SamplerState Sampler, half3 RayPosition, half InvNoiseScale, half3 WindVelocity, half NoiseMin, half NoiseMax, int Octaves)
{
	half3 uvw = RayPosition * InvNoiseScale;
	half3 wind = WindVelocity * InvNoiseScale * _Time.y;
	
	half4 value = 0;
	half c = 0;
	half amp = 1.0;
	
	for (int i = 1; i <= Octaves; i++)
	{
		value += amp * saturate(SAMPLE_TEXTURE3D_LOD(NoiseTexture, Sampler, uvw + wind, 0));
		c += amp;
		uvw *= _Lacunarity;
		amp *= _Gain;
	}
	value /= c;
	
	half fbm = value.r * 0.53 + value.g * 0.27 + value.b * 0.13 + value.a * 0.07;
	
	fbm = saturate(fbm);
	fbm = Remap(NoiseMin, NoiseMax, 0.0, 1.0, fbm);

	return fbm;
}

half GetFogFalloff(float3 RayPosition)
{
	half a = 1.0;
	if(_FogMaskBlendMode == 1)
		a = 0;
	
	for (int i = 0; i < _VolumeCountButo; i++)
	{
		half d = distance(RayPosition, _VolumeDataButo[i].xyz);
		a += Remap(0, _VolumeDataButo[i].w, 1, 0, d) * _VolumeIntensityButo[i];
	}
	
	return a;
}

half CalculateExponentialHeightFog(half extinction, half fogExp, half rayOrigin_Y, half rayLength)
{
	return saturate((extinction / fogExp) * exp(-rayOrigin_Y * fogExp) * (1.0 - exp(-rayLength * rayDirection.y * fogExp)) / rayDirection.y);
}


half2 GetTexCoordSize(float scale)
{
	return 1.0 / (_ScreenParams.xy * scale);
}


void SampleVolumetricFog_half(half2 UV, half3 RayOrigin, 
half MaxDistanceNonVolumetric, half MaxDistanceVolumetric, half Anisotropy,
half BaseHeight, half AttenuationBoundarySize, half FogDensity, half LightIntensity,
half ShadowIntensity, Texture2D ColorRamp, half ColorRampInfluence, Texture3D NoiseTexture,
SamplerState NoiseSampler, half3 NoiseWindVelocity, half NoiseScale, half NoiseMin,
half NoiseMax, bool animateSamplePosition, int SampleCount, SamplerState PointSampler, 
int Octaves, half UnjitteredDepth,
out half3 Color, out half Alpha)
{
	// Out Value Setup
	Color = half3(0.0, 0.0, 0.0);
	Alpha = 1.0;
	
	#ifndef SHADERGRAPH_PREVIEW
	
	
	
	// Initializing and Checking Params
	AttenuationBoundarySize = max(AttenuationBoundarySize, 0.001);
	LightIntensity = max(LightIntensity, 0.0);
	ShadowIntensity = max(ShadowIntensity, 0.0);
	FogDensity = max(FogDensity, 0.001);
	
	// Light Setup
	Light mainLight = GetMainLight();
	mainLightColor = normalize(mainLight.color);
	mainLightDirection = mainLight.direction;
	
	half2 jitteredUV = UV;
	// Ray Setup
	if (animateSamplePosition)
	{
		half2 texCoord = GetTexCoordSize(0.5);
		half2 jitter = texCoord * ((rand2dTo2d(UV + _Time.x) * 2) - 1.0);
		jitteredUV += jitter;
	}
	half depth01 = SampleSceneDepth(UV);
	half depth = LinearEyeDepth(depth01, _ZBufferParams);
	
	half3 viewVector = mul(unity_CameraInvProjection, half4(jitteredUV * 2 - 1, 0.0, -1)).xyz;
	viewVector = mul(unity_CameraToWorld, half4(viewVector, 0.0)).xyz;
	half viewLength = length(viewVector);
	half realDepth = depth * viewLength;
	rayDirection = viewVector / viewLength;
	
	half targetRayDistance = MaxDistanceVolumetric;
	
	// Lighting
	// Default Fog Albedo is registered as half3(1.0, 0.964, 0.92) 
	half extinction = FogDensity * FogDensity * 0.001;
	half hg = HenyeyGreenstein(Anisotropy);
	
	static half points[3] =
	{
		0.165,
		0.495,
		0.825
	};
	
	
	// Ray March
	half2 seed = UV;
	if(animateSamplePosition)
		seed += _Time.x;
	
	half random = rand2dTo1d(seed);
	
	half invStepCount = 1.0 / (half)SampleCount;
	half invNoiseScale = 1.0 / NoiseScale;
	half invTargetRayDistance = 1.0 / targetRayDistance;
	half invMaxRayDistance = 1.0 / MaxDistanceVolumetric;
	
	half3 rayPosition = RayOrigin;
	half lowerLimit, rayDepth_previous, rayDepth_current;
	lowerLimit = 0;
	rayDepth_previous = 0;
	rayDepth_current = 0;
	
	
	for (int i = 1; i <= SampleCount; i++)
	{
		// Positioning
		half ratio = (half)i * invStepCount;
		//half upperLimit = (ratio * ratio) * targetRayDistance; 
		half upperLimit = ratio * targetRayDistance;
		rayDepth_current = lerp(lowerLimit, upperLimit, random);
		lowerLimit = upperLimit;
		
		bool earlyExit = false;
		if (rayDepth_current > realDepth)
		{
			rayDepth_current = realDepth;
			earlyExit = true;
		}
			
		
		rayPosition = RayOrigin + rayDirection * rayDepth_current;
		half stepLength = rayDepth_current - rayDepth_previous;
		rayDepth_previous = rayDepth_current;
		
		// Transmittance
		half relDist = saturate(rayDepth_current * invTargetRayDistance);
		half sampleDensity = GetFogDensityByHeight(AttenuationBoundarySize, BaseHeight, rayPosition) * GetFogDensityByNoise(NoiseTexture, NoiseSampler, rayPosition, invNoiseScale, NoiseWindVelocity, NoiseMin, NoiseMax, Octaves) * GetFogFalloff(rayPosition);
		half e = 1.0;
		#if !_BUTO_ANALYTIC_FOG_ENABLED
		e = lerp(1.0, 0.0, ratio);
		#endif
		
		half sampleExtinction = extinction * sampleDensity * e;
		
		if (sampleExtinction > 0.0001)
		{
			// Evaluate Color
			half3 colorSamples[3];
			for (int i = 0; i <= 2; i++)
			{
				colorSamples[i] = SAMPLE_TEXTURE2D_LOD(ColorRamp, PointSampler, half2(saturate(rayDepth_current * invMaxRayDistance), points[i]), 0).rgb;
			}
			
			half3 shadedColor = lerp(_WorldColor, colorSamples[0], ColorRampInfluence);
			half3 litColor = lerp(_WorldColor + mainLightColor, colorSamples[1], ColorRampInfluence) * LightIntensity;
			half3 emitColor = colorSamples[2] * ColorRampInfluence;
			
			half shadowAttenuation = GetShadowAttenuation(rayPosition);
			half finalShading = shadowAttenuation;
			
			
			#if _BUTO_SELF_ATTENUATION_ENABLED
				half shadowSelfAttenuation = 1.0;
				for (int f = 1; f < 5; f++)
				{
					half selfAttenStepLength = pow(f/5.0, 2.0) * AttenuationBoundarySize;
					half3 currRayPos = rayPosition + mainLightDirection * f * selfAttenStepLength;
					half density = GetFogDensityByHeight(AttenuationBoundarySize, BaseHeight, currRayPos) * GetFogDensityByNoise(NoiseTexture, NoiseSampler, currRayPos, invNoiseScale, NoiseWindVelocity, NoiseMin, NoiseMax, _MaximumSelfShadowingOctaves) * GetFogFalloff(currRayPos);
					shadowSelfAttenuation *= exp(-density * extinction * selfAttenStepLength * 1.0);
				}
				finalShading *= shadowSelfAttenuation;
			#endif
			
			#if _BUTO_HORIZON_SHADOWING_ENABLED
				finalShading *= CalculateHorizonFalloff(rayPosition, mainLightDirection);
			#endif
			
			half shadowIntensity = lerp(ShadowIntensity, 1.0, shadowAttenuation);
			
			half3 fogColor = lerp(shadedColor, litColor * hg, finalShading) + emitColor;
			
			// Solve Transmittance
			half transmittance = exp(-sampleExtinction * stepLength * shadowIntensity);
			half3 additionalLightColor = GetAdditionalLightData(rayPosition);
			
			// Solve Color
			half3 inScatteringData = sampleExtinction * (fogColor + additionalLightColor);
			half3 integratedScattering = (inScatteringData - (inScatteringData * transmittance)) / sampleExtinction;	
			Color += Alpha * integratedScattering;
			
			
			// Apply Transmittance
			Alpha *= transmittance;
			
			if(earlyExit)
				break;
		}
	}
	
	#if _BUTO_ANALYTIC_FOG_ENABLED
		if (MaxDistanceVolumetric < realDepth)
		{
			half s = smoothstep(-MaxDistanceVolumetric * 0.2, 0, realDepth - MaxDistanceVolumetric);
			half sampleDist = MaxDistanceNonVolumetric;
			if (depth01 < 1.0)
			{
				sampleDist = min(sampleDist, realDepth);
			}
		
			half rayLength = sampleDist - rayDepth_current;
			half rayHeight = max(rayPosition.y - BaseHeight, 0.0);
	
			//half rayLength = sampleDist;
			//half rayHeight = max(RayOrigin.y - BaseHeight, 0.0);
	
			// Multiplied by 0.5 to account for the noise texture (average value = 0.5) and by the NoiseMin range^2 to account for the remaining % coverage
			//half e = extinction * 0.25 * ((1.0 - NoiseMin) * (1.0 - NoiseMin)) * lerp(1.0, 1.0-Alpha,0.8);
	
	
			half e = extinction * 0.5;
			half fogAmount = 1.0 - CalculateExponentialHeightFog(e, 1.0 / max(AttenuationBoundarySize, 0.001), rayHeight, rayLength) * s;
		
			// Apply Transmittance
			half alpha_previous = Alpha;
			Alpha *= fogAmount;
	
			// Evaluate Color
			half3 colorSamples[3];
			for (int f = 0; f <= 2; f++)
			{
				colorSamples[f] = SAMPLE_TEXTURE2D_LOD(ColorRamp, PointSampler, half2(1.0, points[f]), 0).rgb;
			}
		
			half shadowAttenuation = 1.0;
	
			#if _BUTO_HORIZON_SHADOWING_ENABLED
				shadowAttenuation *= CalculateHorizonFalloff(rayPosition, mainLightDirection);
			#endif
		
			half3 shadedColor = lerp(_WorldColor, colorSamples[0], ColorRampInfluence);
			half3 litColor = lerp(_WorldColor + mainLightColor, colorSamples[1], ColorRampInfluence) * LightIntensity;
			half3 emitColor = colorSamples[2] * ColorRampInfluence;
		
			half3 fogColor = lerp(shadedColor, litColor * hg, shadowAttenuation) + emitColor;
		
			Color += fogColor * (alpha_previous - Alpha) * s;
		}	
#endif
	#endif
}

#endif