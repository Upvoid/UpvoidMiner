#version 140

#include <Common/Lighting.fsh>

// material:
uniform sampler2D uColor;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vNormal;
in vec3 vEyePos;
in vec3 vColor;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a > uDiscardBias)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha

    texColor.rgb *= vColor;

    vec3 normalFront = mix(vNormal, -vNormal, float(!gl_FrontFacing));

    vec3 colorFront = lighting(vEyePos, normalFront, texColor.rgb, vec4(vec3(0),1));
    vec3 colorBack = lighting(vEyePos, -normalFront, texColor.rgb, vec4(vec3(0),1));

    const float translucency = 1.0;

    vec3 color = colorFront + translucency*colorBack;

    OUTPUT_TransparentColor(vec4(color, texColor.a));
}