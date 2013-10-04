#version 140

#include <Common/Lighting.fsh>

uniform vec4 uColor = vec4(1);
uniform float uScale = 20;
uniform float uScale2 = 5;
uniform float uSpeed = 1;
uniform float uSineOffset = -.4;
uniform float uXAlphaMin = .3;

in vec3 vObjPos;
in vec3 vWorldPos;

in vec3 vRef1;
in vec3 vRef2;

OUTPUT_CHANNEL_TransparentColor(vec4)

void main()
{
    INIT_CHANNELS;

    vec4 transColor = uColor;

    float dis1 = distance(vWorldPos, vRef1);
    float dis2 = distance(vWorldPos, vRef2);
    //float dis1 = distance(vWorldPos.xz, vRef1.xz) + distance(vWorldPos.y, vRef1.y);
    //float dis2 = distance(vWorldPos.xz, vRef2.xz) + distance(vWorldPos.y, vRef2.y);

    const float phase = .982;
    float a1 = cos(dis1 * uScale + uRuntime * uSpeed + phase) + uSineOffset;
    float a2 = cos(dis2 * uScale + uRuntime * uSpeed + phase) + uSineOffset;
    transColor.a *= min(1, max(0, max(a1, a2)) * 1.5 );
    //transColor.a *= cos(vObjPos.y * vObjPos.y * uScale + uRuntime * uSpeed);
    //transColor.a *= transColor.a;
    float modY = 1 - abs(vObjPos.y);
    float modX1 = max(uXAlphaMin, 1 - dis1 / uScale2);
    float modX2 = max(uXAlphaMin, 1 - dis2 / uScale2);
    transColor.a *= min(modY, max(modX1, modX2));

    OUTPUT_TransparentColor(transColor);
}
