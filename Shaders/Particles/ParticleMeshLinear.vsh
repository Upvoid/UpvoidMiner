#version 140

#pragma ACGLimport <Common/Camera.csh>

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

out vec3 vNormal;
out vec3 vTangent;
out vec2 vTexCoord;
out vec4 vColor;
//out float vLife;
out vec3 vWorldPos;

uniform mat4 uModelMatrix;

uniform int uStrideSize;
uniform samplerBuffer uBuffer0;
uniform samplerBuffer uBuffer1;
uniform float uInterpolationFactor;

const int offsets[] = int[]
(
   0, // position vec3
   3, // velocity vec3
   6, // size float
   7, // aCurrentLifetime float
   8, // aMaxLifetime float
   9, // tangent
   12, // bitangent
   15 // angularVelocity
);

vec2 linearInterpolationFactors = vec2(1.0 - uInterpolationFactor, uInterpolationFactor);

int getParticleOffset()
{
   return gl_InstanceID * uStrideSize;
}

float interpolateFloat(int offset)
{
   int currentOffset = getParticleOffset() + offset;

   float float0 = texelFetch(uBuffer0, currentOffset).x;
   float float1 = texelFetch(uBuffer1, currentOffset).x;

   return linearInterpolationFactors.x * float0 +
          linearInterpolationFactors.y * float1;
}

vec2 interpolateVec2(int offset)
{
    float floatX = interpolateFloat(offset);
    float floatY = interpolateFloat(offset+1);

    return vec2(floatX, floatY);
}

vec3 interpolateVec3(int offset)
{
    float floatX = interpolateFloat(offset);
    float floatY = interpolateFloat(offset+1);
    float floatZ = interpolateFloat(offset+2);

    return vec3(floatX, floatY, floatZ);
}

vec4 interpolateVec4(int offset)
{
    float floatX = interpolateFloat(offset);
    float floatY = interpolateFloat(offset+1);
    float floatZ = interpolateFloat(offset+2);
    float floatW = interpolateFloat(offset+3);

    return vec4(floatX, floatY, floatZ, floatW);
}

// a is angle in radians, v does will be normalized
mat3 rotate(mat3 m, vec3 v, float a)
{
   float c = cos(a);
   float s = sin(a);

   vec3 axis = normalize(v);

   vec3 temp = (float(1) - c) * axis;

   mat3 Rotate;
   Rotate[0][0] = c + temp[0] * axis[0];
   Rotate[0][1] = 0 + temp[0] * axis[1] + s * axis[2];
   Rotate[0][2] = 0 + temp[0] * axis[2] - s * axis[1];

   Rotate[1][0] = 0 + temp[1] * axis[0] - s * axis[2];
   Rotate[1][1] = c + temp[1] * axis[1];
   Rotate[1][2] = 0 + temp[1] * axis[2] + s * axis[0];

   Rotate[2][0] = 0 + temp[2] * axis[0] + s * axis[1];
   Rotate[2][1] = 0 + temp[2] * axis[1] - s * axis[0];
   Rotate[2][2] = c + temp[2] * axis[2];

   mat3 result;
   result[0] = m[0] * Rotate[0][0] + m[1] * Rotate[0][1] + m[2] * Rotate[0][2];
   result[1] = m[0] * Rotate[1][0] + m[1] * Rotate[1][1] + m[2] * Rotate[1][2];
   result[2] = m[0] * Rotate[2][0] + m[1] * Rotate[2][1] + m[2] * Rotate[2][2];
   return result;
}

void main()
{
   vec3 iPosition = interpolateVec3(offsets[0]);
   //vec3 iVelocity = interpolateVec3(offsets[1]);
   float iSize = interpolateFloat(offsets[2]);
   float iCurLife = interpolateFloat(offsets[3]);
   float iMaxLife = interpolateFloat(offsets[4]);
   vec3 iTangent = interpolateVec3(offsets[5]);
   vec3 iBiTangent = interpolateVec3(offsets[6]);
   vec3 iAngularVelocity = interpolateVec3(offsets[7]);


   // Compute relative particle life, i.e. \in 0..1
   float iRelLife = iCurLife/iMaxLife;
   iSize *= smoothstep(1, 0.85, iRelLife);
   //vLife = iRelLife;

   // Pass attributes
   vTexCoord = aTexCoord;
   vColor = vec4(1);

   vec3 inormal = cross(iTangent, iBiTangent);

   // TODO(ks) more efficient usage of angularVelocity!
   mat3 localModel = rotate(mat3(inormal, iTangent, iBiTangent), iAngularVelocity, iCurLife * length(iAngularVelocity));

   // world space normal/tangent:
   mat3 normalMatrix = mat3(uModelMatrix) * localModel;
   vNormal = normalMatrix * aNormal;
   vTangent = normalMatrix * aTangent;

   // Create position of particle object vertices, i.e particlePosition + size * rotatedObject
   vec3 objectPos = iPosition + iSize * (localModel * aPosition);
   vec4 worldPos = uModelMatrix * vec4(objectPos, 1);

   vWorldPos = worldPos.xyz;

   // projected vertex position used for the interpolation
   gl_Position = uViewProjectionMatrix * worldPos;
}
