#version 140

uniform mat4 uShadowViewProjectionMatrix;
uniform mat4 uModelMatrix;

in vec3 aPosition;

in vec3 aParticlePosition;
in vec3 aParticleTangent;
in vec3 aParticleBitangent;
in float aParticleSize;
in float aParticleLife;

out float vZ;

void main()
{
    // eye pos
    vec3 pnormal = cross(aParticleTangent, aParticleBitangent);
    mat3 localModel = mat3(pnormal, aParticleTangent, aParticleBitangent);
    vec4 pos = uShadowViewProjectionMatrix * uModelMatrix * vec4(localModel * aPosition * aParticleSize * (1-aParticleLife*aParticleLife) + aParticlePosition, 1.0);

    vZ = pos.z;

    // projected vertex position used for the interpolation
    gl_Position  = pos;
}
