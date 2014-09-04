#version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;

out vec3 vEyePos;
out vec3 vObjectPos;
out vec3 vObjectNormal;

void main()
{
    // object space stuff
    vObjectPos = aPosition;
    vObjectNormal = aNormal;

    // world space position:
    vec4 worldPos = uModelMatrix * vec4(aPosition, 1.0);
    vEyePos = (uViewMatrix * worldPos).xyz;

    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}
