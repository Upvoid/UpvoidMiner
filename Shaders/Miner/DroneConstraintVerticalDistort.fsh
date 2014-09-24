#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>

uniform vec4 uColor = vec4(1);
uniform float uScale = 20;
uniform float uScale2 = 5;
uniform float uSpeed = 1;
uniform float uSineOffset = -.4;
uniform float uXAlphaMin = .0;

in vec3 vObjPos;
in vec3 vWorldPos;

in vec3 vRef1;
in vec3 vRef2;

OUTPUT_CHANNEL_Distortion(vec2)

void main()
{
    INIT_CHANNELS;

    vec2 distort = vec2(0);

    float dis1 = distance(vWorldPos, vRef1);
    float dis2 = distance(vWorldPos, vRef2);
    //float dis1 = distance(vWorldPos.xz, vRef1.xz) + distance(vWorldPos.y, vRef1.y);
    //float dis2 = distance(vWorldPos.xz, vRef2.xz) + distance(vWorldPos.y, vRef2.y);

    const float phase = .982;
    float a1 = cos(dis1 * uScale * 2 + uRuntime * uSpeed + phase) + uSineOffset;
    float a2 = cos(dis2 * uScale * 2 + uRuntime * uSpeed + phase) + uSineOffset;

    distort += normalize(vec2(distance(vWorldPos.xz, vRef1.xz), distance(vWorldPos.y, vRef1.y))) * a1;
    distort += normalize(vec2(distance(vWorldPos.xz, vRef2.xz), distance(vWorldPos.y, vRef2.y))) * a2;

    float modX1 = max(uXAlphaMin, 1 - dis1 / uScale2);
    float modX2 = max(uXAlphaMin, 1 - dis2 / uScale2);
    distort *= max(modX1, modX2);

    //distort = vec2(cos(uRuntime), sin(uRuntime)) * 20;
    distort *= 20;

    OUTPUT_Distortion(distort);
}
