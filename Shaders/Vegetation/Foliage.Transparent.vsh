#version 140

#include <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

in vec3 aInstPosition;
in vec3 aInstNormal;
in vec3 aInstTangent;
in vec3 aInstColor;

out vec3 vNormal;
out vec3 vTangent;
out vec3 vWorldPos;
out vec2 vTexCoord;
out vec3 vColor;

vec3 windOffset(float height, vec3 pos)
{
    float ws = cos(uRuntime + pos.x + pos.y + pos.z);
    return vec3(ws * cos(pos.x + pos.z), 0, ws * cos(pos.y + pos.z)) * max(0, height) * .1;
}

void main()
{
    vColor = aInstColor;

    vec3 instBitangent = normalize(cross(aInstNormal, aInstTangent)) * length(aInstTangent);
    vec3 instTangent = cross(instBitangent, aInstNormal);
    mat3 instRot = mat3(
                instBitangent,
                aInstNormal,
                instTangent
                );
    mat4 instModel = mat4(
                vec4(instBitangent, 0.0),
                vec4(aInstNormal, 0.0),
                vec4(instTangent, 0.0),
                vec4(aInstPosition, 1.0)
                );

    // world space normal:
    vNormal = mat3(uModelMatrix) * instRot * aNormal;
    vTangent = mat3(uModelMatrix) * instRot * aTangent;
    vTexCoord = aTexCoord;

    // world space position:
    vec4 worldPos = uModelMatrix * instModel * vec4(aPosition, 1.0);
    worldPos.xyz += windOffset(aPosition.y, aInstPosition);
    vWorldPos = worldPos.xyz;

    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}
