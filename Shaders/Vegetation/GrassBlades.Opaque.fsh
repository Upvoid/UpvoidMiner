#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Camera.csh>

in vec3 vNormal;
in vec3 vWorldPos;
in vec3 vColor;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void main()
{
    INIT_CHANNELS;

    // illumination
    vec3 normal = normalize(vNormal);

    normal *= sign(dot(uSunDirection, normal));

    vec3 color = lighting(vWorldPos, normal, vColor, vec4(vec3(.3), 16));
    //vec3 color = leafLighting(vWorldPos, normal, 1.0, vColor, vec4(vec3(0.3),16));

    OUTPUT_Color(color);
    OUTPUT_Normal(normal);
    OUTPUT_Position(vWorldPos);
}
