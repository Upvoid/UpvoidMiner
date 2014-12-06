#version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform mat4 uModelMatrix;
uniform float uFadeDistance = 10000;
uniform float uDiscardBias = 0.5;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

in vec3 aInstPosition;
in vec3 aInstNormal;
in vec3 aInstTangent;
in vec3 aInstColor;

out vec2 vTexCoord;
out vec3 vNormal;
out float vTexUnit;
out float vDisc;

vec3 windOffset(float height, vec3 pos)
{
    float ws = cos(uRuntime + pos.x + pos.y + pos.z);
    return vec3(ws * cos(pos.x + pos.z), 0, ws * cos(pos.y + pos.z)) * max(0, height) * .1;
}

void main()
{
   float tanLength = length(aInstTangent);
   vec3 instBitangent = normalize(cross(aInstNormal, aInstTangent));
   vec3 instTangent = normalize(cross(instBitangent, aInstNormal));
   mat4 instModel = mat4(
         vec4(instBitangent * tanLength, 0.0),
         vec4(aInstNormal, 0.0),
         vec4(instTangent * tanLength, 0.0),
         vec4(aInstPosition, 1.0)
         );

   // world space normal:
   vTexCoord = aTexCoord;

   float posFactor = 1 - smoothstep(uFadeDistance * .8, uFadeDistance, distance(aInstPosition, uCameraPosition));

   vec3 vObjPos = (uModelMatrix * vec4(aInstPosition, 1.0)).xyz;

   // tex unit
   const int numberOfFlowerTypes = 5;

   float positionAddition = 0.5 + 0.5 * sin(0.018*vObjPos.x + 0.003*vObjPos.z + cos(0.1*vObjPos.z + 0.01 * vObjPos.x));
   positionAddition *= numberOfFlowerTypes;

   vTexUnit = int(positionAddition + 0.5) % 15;

   // world space position:
   vec4 worldPos = uModelMatrix * (instModel * vec4(aPosition * posFactor, 1.0));
   worldPos.xyz += windOffset(aPosition.y, aInstPosition);

   // disc
   float disc = uDiscardBias;
   disc = distance(worldPos.xyz, uCameraPosition);
   disc = (1-uDiscardBias) + 0.05-clamp(disc/50,0,1-uDiscardBias);
   vDisc = disc;

   vNormal = aInstNormal;

   // projected vertex position used for the interpolation
   gl_Position  = uViewProjectionMatrix * worldPos;
}
