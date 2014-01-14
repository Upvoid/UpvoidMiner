#version 140

#include <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;

out vec3 vObjPos;
out vec3 vEyePos;

void main()
{
    // world space position:
    vObjPos = aPosition;
    vEyePos = vec3(uViewMatrix * uModelMatrix * vec4(aPosition, 1.0));

    // projected vertex position used for the interpolation
    gl_Position  = uProjectionMatrix * vec4(vEyePos, 1.0);
}
