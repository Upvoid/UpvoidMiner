#version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform float uFadeDistance = 10000;

in vec3 aPosition;
in vec3 aXYR;

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
   float aX = aXYR.x;
   float aY = aXYR.y;
   float aR = aXYR.z;

   // pass-through
   vX = aX;
   vY = aY;
   vR = aR;

   // transformation
   vWorldPos = aPosition + windOffset(aPosition, aY);

   // projected vertex position used for the interpolation
   gl_Position  = uViewProjectionMatrix * vec4(vWorldPos, 1.0);
}
