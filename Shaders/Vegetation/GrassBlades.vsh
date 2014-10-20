#version 140

#pragma ACGLimport  <Common/Camera.csh>

in vec3 aPosition;
in vec3 aXYR;

out vec3 vWorldPos;
out float vX;
out float vY;
out float vR;

vec3 windOffset(vec3 pos, float l, float offs)
{
   float ws = cos(uRuntime + pos.x + pos.y + pos.z + offs);
         //+ max(0.1, sin(uRuntime * (3 + cos(dot(pos, vec3(0.11, 0.09, 0.1) / 5)))) - .5) * 3 * (0.5 + 0.5 * cos(uRuntime + pos.x * 0.13 + pos.y * 0.53 + pos.z * .09));
   return vec3(ws * cos(pos.x + pos.z + offs), 0, ws * cos(pos.y + pos.z + offs)) * l * .1;
}

vec3 windOffset2(vec3 pos, float l, float offs)
{
   float angle = cos(dot(pos, vec3(0.11, 0.09, 0.1)*3));
   float ca = cos(angle);
   float sa = sqrt(1 - ca*ca);
   float speed = 0.6;
   return vec3(
            ca * cos(uRuntime * speed + pos.x + pos.z + offs),
            0,
            sa * cos(uRuntime * speed + pos.y + pos.z + offs))
         * l * .08;
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
   vWorldPos = aPosition + windOffset2(aPosition, aY, aR * 2);

   // projected vertex position used for the interpolation
   gl_Position  = uViewProjectionMatrix * vec4(vWorldPos, 1.0);
}
