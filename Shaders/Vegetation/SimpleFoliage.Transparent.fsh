#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;
uniform float uFadeDistance = 10000;

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vColor;
in vec3 vWorldPos;
in vec3 vInstNormal;
in vec3 vInstPosition;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a > uDiscardBias || texColor.a < 0.004)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha

    texColor.rgb *= vColor;

    vec3 normal = normalize(vNormal + vInstNormal * 1.9);

    vec3 color = leafLighting(vWorldPos, normal, 1.0, texColor.rgb, vec4(vec3(0),1));

    float posFactor = 1 - smoothstep(uFadeDistance * 0.5, uFadeDistance * 0.8, distance(vInstPosition, uCameraPosition));

    OUTPUT_TransparentColor(vec4(color, texColor.a * posFactor));
}
