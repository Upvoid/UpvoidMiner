#version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform mat4 uModelMatrix;
uniform float uFadeDistance = 10000;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aColor;
in float aLength;

out vec3 vNormal;
out vec3 vColor;
out vec3 vWorldPos;

vec3 windOffset(vec3 pos, float l)
{
   float ws = cos(uRuntime + pos.x + pos.y + pos.z);
   return vec3(ws * cos(pos.x + pos.z), 0, ws * cos(pos.y + pos.z)) * l * .1;
}

void main()
{
   vColor = aColor;
   // world space normal:
   vNormal = mat3(uModelMatrix) * aNormal;
   // world space position:
   vec4 worldPos = uModelMatrix * vec4(aPosition, 1.0);

   vWorldPos = worldPos.xyz;// + windOffset(worldPos.xyz, aLength);

   // projected vertex position used for the interpolation
   gl_Position  = uViewProjectionMatrix * vec4(vWorldPos, 1.0);
}
