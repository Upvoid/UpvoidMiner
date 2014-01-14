#version 140

#include <Common/Lighting.fsh>

uniform vec4 uColor = vec4(1);
uniform float uScale = 10;
uniform float uSpeed = 1;

in vec3 vObjPos;
in vec3 vEyePos;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    INIT_CHANNELS;

    vec4 transColor = uColor;

    transColor.a *= cos(vObjPos.y * vObjPos.y * uScale + uRuntime * uSpeed);
    transColor.a *= transColor.a;
    transColor.a *= 1 - abs(vObjPos.y);

    OUTPUT_TransparentColor(transColor);
}
