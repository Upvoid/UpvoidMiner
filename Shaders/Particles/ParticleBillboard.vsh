#version 140

#pragma ACGLimport  <Common/Camera.csh>
#pragma ACGLimport  <Common/Lighting.fsh>

in vec3 aPosition;
in vec2 aTexCoord;

in vec3 aParticlePosition;
in vec4 aParticleColor;
in float aParticleSize;
in float aParticleAngle;
in float aParticleLife;

out vec3 vNormal;
out vec3 vWorldPos;
out vec4 vParticleColor;
out vec2 vTexCoord;
out vec4 vScreenPos;
out float vLife;

uniform mat4 uModelMatrix;

void main()
{
    vTexCoord = aTexCoord;

    vParticleColor = clamp(aParticleColor, 0, 1);

    // world space position:
    vec4 worldPos = uModelMatrix * vec4(aParticlePosition, 1.0);
    vWorldPos = worldPos.xyz;
    vLife = aParticleLife;

    vec4 eyePos = uViewMatrix * worldPos;

    vec2 tangent = vec2(
                cos(aParticleAngle),
                sin(aParticleAngle)
                );
    vec2 normal = vec2(
                -tangent.y,
                tangent.x
                );

    float size = aParticleSize * (0.25 + 0.75 * smoothstep(0.0, 0.7, aParticleLife));

    size *= 1 + 1 * smoothstep(0.7, 1.0, aParticleLife);

    eyePos.xy += tangent * (aPosition.x * size)
                + normal * (aPosition.y * size);

    vNormal = vec3(uInverseViewMatrix * vec4(0, 0, -1, 0));

    // projected vertex position used for the interpolation
    vScreenPos = uProjectionMatrix * eyePos;
    gl_Position  = vScreenPos;
}
