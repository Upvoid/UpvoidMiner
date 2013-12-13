#version 140

#include <Common/Lighting.fsh>
#include <Common/Camera.csh>
#include <Common/Normalmapping.fsh>

// material:
uniform sampler2D uColor;
uniform sampler2D uNormal;
uniform sampler2D uTranslucency;

uniform float uDiscardBias = 0.5;

in vec2 vTexCoord;
in vec3 vTangent;
in vec3 vNormal;
in vec3 vWorldPos;
in vec3 vColor;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)

void main()
{
    vec4 texColor = texture(uColor, vTexCoord);

    if(texColor.a < uDiscardBias)
        discard;

    texColor.rgb /= texColor.a + 0.001; // premultiplied alpha


    vec3 normalFront = vNormal;

    if(!gl_FrontFacing)
        normalFront = -normalFront;

    // The vertex shader flips the normal so it always points towards the camera
    normalFront = applyNormalmap(normalFront, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));
    vec3 normalBack = applyNormalmap(-normalFront, vTangent, unpack8bitNormalmap(texture(uNormal, vTexCoord).rgb));

    texColor.rgb *= vColor;

    vec3 colorFront = lighting(vWorldPos, normalFront, texColor.rgb, vec4(vec3(0),1));
    vec3 colorBack = lighting(vWorldPos, normalBack, texColor.rgb, vec4(vec3(0),1));

    vec3 translucency = texture(uTranslucency, vTexCoord).rgb;

    vec3 color = colorFront + translucency*colorBack;

    OUTPUT_Color(color);
    OUTPUT_Normal(normalFront);
}
