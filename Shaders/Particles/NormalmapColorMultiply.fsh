#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Normalmapping.fsh>

uniform sampler2D uColor;
uniform sampler2D uNormal;

uniform float uRoughness = 0.5;
uniform float uFresnel = 1.3;
uniform float uGlossiness = 0.5;

in vec3 vNormal;
in vec3 vTangent;
in vec3 vWorldPos;
in vec2 vTexCoord;
in vec4 vColor;

OUTPUT_CHANNEL_GBuffer1(vec4)
OUTPUT_CHANNEL_GBuffer2(vec4)

void main()
{
    INIT_CHANNELS;

    // normalmap
    vec3 normal = applyNormalmap(vNormal, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));

    // illumination
    vec3 baseColor = texture(uColor, vTexCoord).rgb;
    baseColor *= vColor.rgb * vColor.a;

    vec4 gb1, gb2;
    writeGBuffer(
       baseColor.rgb,
       normal,
       uRoughness,
       uFresnel,
       uGlossiness,
       gb1, gb2
    );
    OUTPUT_GBuffer1(gb1);
    OUTPUT_GBuffer2(gb2);
}
