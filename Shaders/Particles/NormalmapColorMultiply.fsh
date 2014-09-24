#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Normalmapping.fsh>

uniform sampler2D uColor;
uniform sampler2D uNormal;
uniform vec4 uSpecularColor;

in vec3 vNormal;
in vec3 vTangent;
in vec3 vWorldPos;
in vec2 vTexCoord;
in vec4 vColor;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void main()
{
    INIT_CHANNELS;

    // normalmap
    vec3 normal = applyNormalmap(vNormal, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));

    // illumination
    vec3 baseColor = texture(uColor, vTexCoord).rgb;
    baseColor *= vColor.rgb * vColor.a;
    vec3 color = lighting(vWorldPos, normal, baseColor, uSpecularColor);

    OUTPUT_Color(color);
    OUTPUT_Normal(normal);
    OUTPUT_Position(vWorldPos);
}
