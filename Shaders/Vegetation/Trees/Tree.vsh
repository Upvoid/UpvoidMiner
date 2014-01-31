#version 140

#include <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

out vec3 vNormal;
out vec3 vTangent;
out vec3 vEyePos;
out vec2 vTexCoord;

vec3 windOffset(float height, vec3 pos)
{
    float ws = cos(uRuntime + pos.x + pos.y + pos.z);
    return vec3(ws * cos(pos.x + pos.z), 0, ws * cos(pos.y + pos.z)) * max(0, height) * .1;
}

void main()
{
    // world space normal:
    vNormal = mat3(uModelMatrix) * aNormal;
    vTangent = mat3(uModelMatrix) * aTangent;
    vTexCoord = aTexCoord;
    // world space position:
    vec4 worldPos = uModelMatrix * vec4(aPosition, 1.0);
    worldPos.xyz += windOffset(0.5*length(aPosition.xz)*clamp(aPosition.y-0.2, 0, 1), worldPos.xyz/6);
    vEyePos = (uViewMatrix * worldPos).xyz;

    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}

