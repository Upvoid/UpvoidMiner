#version 140

#pragma ACGLimport <Common/Camera.csh>

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

out vec2 vTexCoord;
out vec3 vWorldPos;
out vec3 vEyePos;
out vec4 vScreenPos;
out vec3 iWorldPos;
out float quadID;

uniform mat4 uModelMatrix;

uniform float uRandom;


void main()
{
   // Pass attributes
   vTexCoord = aTexCoord;

   quadID = uRandom + int(gl_VertexID) / 4;


   vec3 pos = aPosition;
   pos.y *= 0.9 + 0.05 * sin(3*uRuntime + 123.2 * quadID + 5 * cos(12*uRandom + uRuntime));


   // world space position:
   vWorldPos = (uModelMatrix * (vec4(pos, 1))).xyz;

   iWorldPos = vec3(uModelMatrix[3]);

   vEyePos = (uViewMatrix * vec4(vWorldPos,1)).xyz;

   // projected vertex position used for the interpolation
   vScreenPos = uProjectionMatrix * vec4(vEyePos,1);

   gl_Position  = vScreenPos;
}
