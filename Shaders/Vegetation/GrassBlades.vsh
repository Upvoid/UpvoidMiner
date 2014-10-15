#version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform float uFadeDistance = 10000;

in vec4 aPositionAndX;
in vec4 aNormalAndYAndR;

out vec3 vNormal;
out vec3 vWorldPos;
out float vX;
out float vY;
out float vR;

vec3 windOffset(vec3 pos, float l)
{
   float ws = cos(uRuntime + pos.x + pos.y + pos.z);
   return vec3(ws * cos(pos.x + pos.z), 0, ws * cos(pos.y + pos.z)) * l * .1;
}

void main()
{
   // input multiplex
   vec3 aPosition = aPositionAndX.xyz;
   float aX = aPositionAndX.w;
   vec3 aNormal = vec3(aNormalAndYAndR.x, 0.0, aNormalAndYAndR.y);
   aNormal.y = sqrt(1 - aNormal.x * aNormal.x - aNormal.z * aNormal.z);
   float aY = aNormalAndYAndR.z;
   float aR = aNormalAndYAndR.w;

   // pass-through
   vX = aX;
   vY = aY;
   vR = aR;

   // transformation
   vNormal = aNormal;
   vWorldPos = aPosition + windOffset(aPosition, aY) * 0;

   // projected vertex position used for the interpolation
   gl_Position  = uViewProjectionMatrix * vec4(vWorldPos, 1.0);
}
