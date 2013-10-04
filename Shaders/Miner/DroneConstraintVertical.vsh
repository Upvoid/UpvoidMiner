#version 140

#include <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;

out vec3 vObjPos;
out vec3 vWorldPos;
out vec3 vRef1;
out vec3 vRef2;

void main()
{
    // world space position:
    vObjPos = aPosition;
    vWorldPos = vec3(uModelMatrix * vec4(aPosition, 1.0));

    vRef1 = vec3(uModelMatrix * vec4(-1, 0, 0, 1));
    vRef2 = vec3(uModelMatrix * vec4(1, 0, 0, 1));

    // projected vertex position used for the interpolation
    vec4 eyePos = uViewMatrix * vec4(vWorldPos, 1.0);
    eyePos.z += .01; // Workaround for z-Fighting with terrain.
    gl_Position  = uProjectionMatrix * eyePos;
}
