#version 140

#include <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;

out vec3 vNormal;
out vec3 vEyePos;
out vec3 vWorldPos;

void main()
{
    // world space normal:
    vNormal = mat3(uModelMatrix) * aNormal;
    // world space position:
    vWorldPos = vec3(uModelMatrix * vec4(aPosition, 1.0));
    // eye space position:
    vEyePos = vec3(uViewMatrix * vec4(vWorldPos,1));

    // projected vertex position used for the interpolation
    gl_Position  = uProjectionMatrix * vec4(vEyePos, 1.0);

    // avoid z-clipping :D
    gl_Position.z = 0.0;
}
