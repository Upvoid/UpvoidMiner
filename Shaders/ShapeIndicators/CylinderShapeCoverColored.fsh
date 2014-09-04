#version 140

#pragma ACGLimport <Common/Lighting.fsh>

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


    float depth = texture(uOpaqueDepth, gl_FragCoord.xy).r;
    vec4 screenPos = uProjectionMatrix * vec4(vEyePos, 1);
    screenPos /= screenPos.w;
    vec4 opaqueEyePos = uInverseProjectionMatrix * vec4(screenPos.xy, depth * 2 - 1, 1.0);
    opaqueEyePos /= opaqueEyePos.w;

    // get distance to sphere midpoint
	vec3 midEyeDis = vec3(uInverseViewMatrix*vec4(opaqueEyePos.xyz,1)) - uMidPointAndRadius.xyz;
	float dX = abs(dot(midEyeDis, uDigDirX.xyz));
	float dY = abs(dot(midEyeDis, uDigDirY.xyz));
	float dZ = abs(dot(midEyeDis, uDigDirZ.xyz));
    float dist = max(dY, length(vec2(dX, dZ)));


    float e0 = max(0.0, uMidPointAndRadius.w-0.3);
    float e1 = max(0.5, uMidPointAndRadius.w-0.1);
    transColor.a *= smoothstep(e0, e1, dist);
    transColor.a += 0.3*uColor.a;
    transColor.a *= smoothstep(1.0, 0.95, dist/uMidPointAndRadius.w);

    transColor.a *= 0.1 + 0.9 * shadowFactor(vWorldPos);

    /*
    vec3 pos = opaqueWorldPos.xyz;
    const float scale = 30;
    transColor.a += max(0, sin(pos.x * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.y * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.z * scale) - .75) * 4;
    transColor.a = min(1, transColor.a);
    */

    OUTPUT_TransparentColor(transColor);
}
