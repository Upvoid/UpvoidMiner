#version 140

#include <Common/Camera.csh>

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

in vec3 aParticlePosition;
in float aParticleSize;
in vec3 aParticleTangent;
in vec3 aParticleBitangent;
in vec4 aParticleColor;
in float aParticleLife;

out vec3 vNormal;
out vec3 vTangent;
out vec3 vWorldPos;
out vec2 vTexCoord;
out vec4 vColor;

uniform mat4 uModelMatrix;

void main()
{
    vec3 pnormal = cross(aParticleTangent, aParticleBitangent);
    mat3 localModel = mat3(pnormal, aParticleTangent, aParticleBitangent);

    // world space normal:
    mat3 normalMatrix = mat3(uModelMatrix) * localModel;
    vNormal = normalMatrix * aNormal;
    vTangent = normalMatrix * aTangent;
    vTexCoord = aTexCoord;
    vColor = aParticleColor;

    // world space position:
    vec4 worldPos = uModelMatrix * vec4(localModel * aPosition * aParticleSize * (1-aParticleLife*aParticleLife) + aParticlePosition, 1.0);
    vWorldPos = worldPos.xyz;

    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}
