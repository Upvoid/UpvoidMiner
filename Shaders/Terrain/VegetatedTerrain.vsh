 #version 140

#pragma ACGLimport  <Common/Camera.csh>

uniform mat4 uModelMatrix;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aColor;
in float aGrass;

out float vGrass;
out vec3 vColor;
out vec3 vWorldPos;
out vec3 vObjectPos;
out vec3 vObjectNormal;
out vec3 vWorldNormal;
out vec3 vScaling;

void main()
{
    vGrass = aGrass;
    vColor = aColor;

    // object space stuff
    vObjectPos = aPosition;
    vObjectNormal = aNormal;
    vWorldNormal = mat3(uModelMatrix) * aNormal;

    // world space position:
    vec4 worldPos = uModelMatrix * vec4(aPosition, 1.0);
    vWorldPos = worldPos.xyz;
    
    vScaling = vec3(length(uModelMatrix[0]), length(uModelMatrix[1]), length(uModelMatrix[2]));

    // projected vertex position used for the interpolation
    gl_Position  = uViewProjectionMatrix * worldPos;
}
