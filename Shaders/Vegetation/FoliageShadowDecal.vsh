#version 140

#pragma ACGLimport  <Common/Lighting.fsh>

uniform mat4 uModelMatrix;
uniform float uFadeDistance = 10000;

in vec3 aPosition;
in vec3 aNormal;
in vec3 aTangent;
in vec2 aTexCoord;

in vec3 aInstPosition;
in vec3 aInstNormal;
in vec3 aInstTangent;
in vec3 aInstColor;

out vec2 vTexCoord;

out vec4 vProj;

out float vShadowDist;

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
    mat3 instRot = mat3(
                instBitangent,
                normalize(aInstNormal),
                instTangent
                );

    vTexCoord = aTexCoord;

    float posFactor = 1 - smoothstep(uFadeDistance * .8, uFadeDistance, distance(aInstPosition, uCameraPosition));

    // world space position:
	vec3 sunDir = normalize(uSunDirection);
	// TODO(ks) what about sunDir.y = 0?
	vShadowDist = -aPosition.y/sunDir.y;
	vec3 groundProjectedPosition = instRot * aPosition + vShadowDist * sunDir;
	
	vShadowDist = distance(aPosition, groundProjectedPosition);
	groundProjectedPosition.y += 0.001;
    vec4 worldPos = uModelMatrix * vec4(groundProjectedPosition * posFactor, 1.0);
    worldPos.xyz += aInstPosition;// + windOffset(aPosition.y, aInstPosition);

	vProj = uViewProjectionMatrix * worldPos;
    // projected vertex position used for the interpolation
    gl_Position  = vProj;
}
