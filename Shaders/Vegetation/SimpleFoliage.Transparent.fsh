#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vColor;
in vec3 vWorldPos;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a > uDiscardBias || texColor.a < 0.004)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha

    texColor.rgb *= vColor;

    vec3 normalFront = mix(vNormal, -vNormal, float(!gl_FrontFacing));

    // TODO(ks) only one shadow computation!
    vec3 colorFront = lighting(vWorldPos, normalFront, texColor.rgb, vec4(vec3(0),1));
    vec3 colorBack = lighting(vWorldPos, -normalFront, texColor.rgb, vec4(vec3(0),1));

    const float translucency = 1.0;

    vec3 color = colorFront + translucency*colorBack;

    OUTPUT_TransparentColor(vec4(color, texColor.a));
}
