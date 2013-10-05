#version 140

#include <Common/Lighting.fsh>
#include <Common/Particles.fsh>

uniform sampler2D uColor;
uniform vec4 uColorFilter;

uniform sampler2DRect uOpaqueDepth;

in vec4 vParticleColor;

in vec3 vNormal;
in vec3 vWorldPos;
in vec2 vTexCoord;
in vec4 vScreenPos;
in float vLife;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    INIT_CHANNELS;

    // illumination
    vec4 baseColor = texture(uColor, vTexCoord);
    baseColor.rgb /= baseColor.a + 0.001; // premultiplied alpha
    vec4 transColor = baseColor * vParticleColor * uColorFilter;

    vec4 eyePos = uInverseProjectionMatrix * vScreenPos;
    eyePos /= eyePos.w;
    vec4 worldPos = uInverseViewMatrix * eyePos;

    transColor.a *= softParticleFactor(vScreenPos, uOpaqueDepth, 1.0);

    transColor.a *= 1-smoothstep(0.5, 1, vLife);
    transColor.a *= smoothstep(0.0, .2, vLife);

    // DEBUG: no illumination!
    //transColor.rgb *= shadowFactor(worldPos.xyz);

    OUTPUT_TransparentColor(transColor);
}
