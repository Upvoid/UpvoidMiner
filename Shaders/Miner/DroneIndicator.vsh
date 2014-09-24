#version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;

out vec3 vObjPos;
out vec3 vWorldPos;

void main()
{
    // world space position:
    vObjPos = aPosition;
    vWorldPos = vec3(uModelMatrix * vec4(aPosition, 1.0));

    // projected vertex position used for the interpolation
    gl_Position  = uProjectionMatrix * vec4(vWorldPos, 1.0);
}
