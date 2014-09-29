#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

uniform float uOutputOffset;
uniform vec4 uColor;

uniform vec4 uMidPointAndRadius;
uniform sampler2DRect uInDepth;

in vec3 vNormal;
in vec3 vEyePos;
in vec3 vWorldPos;

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
    float dist = distance(opaqueEyePos.xyz, vec3(uViewMatrix*vec4(uMidPointAndRadius.xyz,1)));


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

    /*
    vec3 pos = opaqueWorldPos.xyz;
    const float scale = 30;
    transColor.a += max(0, sin(pos.x * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.y * scale) - .75) * 4;
    transColor.a += max(0, sin(pos.z * scale) - .75) * 4;
    transColor.a = min(1, transColor.a);
    */


    transColor = clamp(transColor, 0, 1);
    OUTPUT_VEC4_OutputColor(transColor);
}
