#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

uniform float uOutputOffset;
uniform vec4 uColor;

uniform sampler2DRect uInDepth;

uniform vec4 uMidPointAndRadius;
uniform vec4 uCursorPos;
uniform vec4 uDigDirX;
uniform vec4 uDigDirY;
uniform vec4 uDigDirZ;
uniform vec4 uTerrainNormal;

in vec3 vNormal;
in vec3 vEyePos;
in vec3 vWorldPos;
in vec3 vWorldCenter;

INPUT_CHANNEL_OutputColor(vec3)
INPUT_CHANNEL_Depth(float)
OUTPUT_CHANNEL_OutputColor(vec3)

void main()
{
    INIT_CHANNELS;

    vec4 transColor = uColor;
    
    // get current depth
    float depth = texture(uInDepth, gl_FragCoord.xy + vec2(uOutputOffset)).r;
    vec4 screenPos = uProjectionMatrix * vec4(vEyePos, 1);
    screenPos /= screenPos.w;

    // get (underlying) opaque fragment position in eye-space
    vec4 opaqueEyePos = uInverseProjectionMatrix * vec4(screenPos.xy, depth * 2 - 1, 1.0);
    opaqueEyePos /= opaqueEyePos.w;

    // get distance to sphere midpoint
	vec3 opaqueWorldPos = (uInverseViewMatrix*vec4(opaqueEyePos.xyz,1)).xyz;
	vec3 midEyeDis = opaqueWorldPos - uMidPointAndRadius.xyz;
	float dX = abs(dot(midEyeDis, uDigDirX.xyz));
	float dY = abs(dot(midEyeDis, uDigDirY.xyz));
	float dZ = abs(dot(midEyeDis, uDigDirZ.xyz));
   
   vec3 diff = vWorldPos - vWorldCenter;
   diff -= vNormal * dot(diff, vNormal);
   float gX = dot(diff, uDigDirX.xyz);
   float gY = dot(diff, uDigDirY.xyz);
   float gZ = dot(diff, uDigDirZ.xyz);
   
   vec3 gNormal = vec3(
      dot(vNormal, uDigDirX.xyz),
      dot(vNormal, uDigDirY.xyz),
      dot(vNormal, uDigDirZ.xyz)
   );
   
   float gS = uMidPointAndRadius.w;
   
   diff = abs(0.5 - fract(vec3(gX, gY, gZ) / gS + .5));
   diff = smoothstep(vec3(0.05), vec3(0.0), diff * gS);
   diff *= (1 - gNormal);
   diff = clamp(diff, vec3(0), vec3(1));
   
   transColor.rgb = vec3(0, .8, 1);
   //transColor.rgb = diff;
   //dot(diff, vec3(1))

   transColor.a = min(1, diff.r + diff.g + diff.b);
   float dl = length(vWorldPos - uCursorPos.xyz);
   transColor.a *= max(0, 1 - dl / (3.5 * gS));
   //transColor.a = 0.2;
   float nX = dot(uTerrainNormal.xyz, uDigDirX.xyz);
   float nY = dot(uTerrainNormal.xyz, uDigDirY.xyz);
   float nZ = dot(uTerrainNormal.xyz, uDigDirZ.xyz);
   vec3 absNormal = abs(vec3(nX, nY, nZ));   
   float maxAbsN = max(absNormal.x, max(absNormal.y, absNormal.z));
   /*vec3 binNormal = vec3(
      float(absNormal.x == maxAbsN),
      float(absNormal.y == maxAbsN),
      float(absNormal.z == maxAbsN)
   );*/
   vec3 binNormal = vec3(
      smoothstep(0.2, 0, maxAbsN - absNormal.x),
      smoothstep(0.2, 0, maxAbsN - absNormal.y),
      smoothstep(0.2, 0, maxAbsN - absNormal.z)
   );
   transColor.a *= dot(binNormal, gNormal);
	
   // Do not show anything behind material
   float depthDiff = opaqueEyePos.z - vEyePos.z;
   float fa = mix(1, -10 , float(depthDiff > 0) * depthDiff);
   transColor.a *= clamp(fa, 0.1, 1);
   
   transColor.rgb = mix(vec3(0, 0.8, 1), vec3(1, 0, 0), 1 - fa);
   //transColor.rgb = absNormal;
   //transColor.rgb = vec3(gZ);
   
   transColor.a *= .2; 
   
	
    transColor = clamp(transColor, 0, 1);
    OUTPUT_VEC4_OutputColor(transColor);
}
