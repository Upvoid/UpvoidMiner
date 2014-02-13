#version 140

#include <Common/Lighting.fsh>

uniform vec4 uColor;

uniform sampler2DRect uOpaqueDepth;

uniform vec4 uMidPointAndRadius;
uniform vec4 uDigDirX;
uniform vec4 uDigDirY;
uniform vec4 uDigDirZ;

in vec3 vNormal;
in vec3 vEyePos;
in vec3 vWorldPos;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    INIT_CHANNELS;

    vec4 transColor = uColor;

    // get current depth
    float depth = texture(uOpaqueDepth, gl_FragCoord.xy).r;
    vec4 screenPos = uProjectionMatrix * vec4(vEyePos, 1);
    screenPos /= screenPos.w;

    // get (underlying) opaque fragment position in eye-space
    vec4 opaqueEyePos = uInverseProjectionMatrix * vec4(screenPos.xy, depth * 2 - 1, 1.0);
    opaqueEyePos /= opaqueEyePos.w;

    // get distance to sphere midpoint
	vec3 midEyeDis = vec3(uInverseViewMatrix*vec4(opaqueEyePos.xyz,1)) - uMidPointAndRadius.xyz;
	float dX = abs(dot(midEyeDis, uDigDirX.xyz));
	float dY = abs(dot(midEyeDis, uDigDirY.xyz));
	float dZ = abs(dot(midEyeDis, uDigDirZ.xyz));
    float dist = max(dX, max(dY, dZ));


    float e0 = max(0.0, uMidPointAndRadius.w-0.1);
    float e1 = max(0.1, uMidPointAndRadius.w-0.0);
    transColor.a *= smoothstep(e0, e1, dist);
    transColor.a += 0.3*uColor.a;

    transColor.a *= float(dist < uMidPointAndRadius.w);

    float sphereAlpha = 0.2;

    // fresnel
    vec3 viewDir = normalize(uCameraPosition - vWorldPos);
    float dotVN = pow(abs(dot(viewDir, vNormal)), 0.5);
    sphereAlpha *= mix(1.0, 0.1, dotVN);

    transColor.a += float(opaqueEyePos.z < vEyePos.z) * sphereAlpha;

    transColor.a *= 0.1 + 0.9*shadowFactor(vWorldPos);
    /*
    vec3 pos = opaqueWorldPos.xyz;
    const float scale = 30;
    transColor.a += max(0, sin(pos.x * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.y * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.z * scale) - .75) * 4;
    transColor.a = min(1, transColor.a);
    */

    OUTPUT_TransparentColor(clamp(transColor, 0, 1));
}
