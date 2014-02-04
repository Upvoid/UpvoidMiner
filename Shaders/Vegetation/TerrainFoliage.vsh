 #version 140

#include <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aColor;
in float aOffset;

out vec3 vColor;
out vec3 vEyePos;
out vec3 vObjectPos;
out vec3 vObjectNormal;
out float vOffset;

void main()
{
    vColor = aColor;
    vOffset = aOffset;

    // object space stuff
    vObjectPos = aPosition;
    vObjectNormal = aNormal;

    // world space position:
    vec4 worldPos = uModelMatrix * vec4(aPosition, 1.0);
    vEyePos = (uViewMatrix * worldPos).xyz;

    float dist = length(worldPos.xyz);
    worldPos.y += .1*(0.5 + 0.5*sin(0.6 * uRuntime + dist));
    worldPos.y += .1*(0.5 + 0.5*cos(0.6 * uRuntime + dist));

    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}
