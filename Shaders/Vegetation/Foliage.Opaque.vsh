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
out vec3 vColor;
out vec3 vTangent;
out vec3 vWorldPos;
out vec2 vTexCoord;

void main()
{
    vColor = aInstColor;

    vec3 instBitangent = normalize(cross(aInstNormal, aInstTangent));
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
    vWorldPos = worldPos.xyz;

    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}
