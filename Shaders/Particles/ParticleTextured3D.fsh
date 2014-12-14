#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Particles.fsh>

uniform sampler3D uColor;

uniform sampler2DRect uOpaqueDepth;

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
    vec4 baseColor = texture(uColor, vec3(vTexCoord.xy, vLife));

    //baseColor.a *= 1.0 - 2*distance(vTexCoord.xy, vec2(0.5));
    baseColor.rgb /= baseColor.a + 0.1; // premultiplied alpha
    vec4 transColor = baseColor;

    float softPart = softParticleFactor(vScreenPos, uOpaqueDepth, 1.0);

    //transColor.a *= softPart;

    transColor.rgb /= 20*distance(vTexCoord, vec2(0.5)) + 0.001;

    //transC

    //transColor.rgba = vec4(vLife, 0.0, 0.0, 1.0);
    OUTPUT_TransparentColor(transColor);
}
