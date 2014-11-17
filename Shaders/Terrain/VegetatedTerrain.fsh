#version 140
#pragma Pipeline

#pragma ACGLimport <Common/Lighting.fsh>
#pragma ACGLimport <Common/Normalmapping.fsh>

uniform sampler2D uColorXY;
uniform sampler2D uColorXZ;
uniform sampler2D uColorXZDirt;
uniform sampler2D uColorZY;
uniform sampler2D uNormalXY;
uniform sampler2D uNormalXZ;
uniform sampler2D uNormalXZDirt;
uniform sampler2D uNormalZY;

uniform vec4 uSpecularColor;

uniform float uTexScaleXY;
uniform float uTexScaleXZ;
uniform float uTexScaleZY;
uniform float uBlendingCoefficient;
uniform float uLodRefDist;
uniform float uLodFactor;
uniform float uLodTransitionPart;

uniform float uBlackness = 1.0;

uniform mat4 uModelMatrix;

in vec3 vColor;
in vec3 vWorldPos;
in vec3 vObjectPos;
in vec3 vObjectNormal;
in vec3 vWorldNormal;
in float vGrass;

OUTPUT_CHANNEL_Color(vec3)
OUTPUT_CHANNEL_Normal(vec3)
OUTPUT_CHANNEL_Position(vec3)

void terrainFunction(float scale, out vec3 color, out vec3 normal)
{
    // texturing
    vec3 xyColor = texture(uColorXY, vObjectPos.xy / (uTexScaleXY * scale)).rgb;
    vec3 xzColor1 = texture(uColorXZ, vObjectPos.xz / (uTexScaleXZ * scale)).rgb;
    vec3 xzColor2 = texture(uColorXZDirt, vObjectPos.xz / (uTexScaleXZ * scale)).rgb;
    vec3 xzColor = mix(xzColor2, xzColor1, min(1, vGrass * 2));
    vec3 zyColor = texture(uColorZY, vObjectPos.zy / (uTexScaleZY * scale)).rgb;

    // for AMD, pow(vec3(0, y, z), w) always returns zero for non-const vec3.
    // this is a hotfix.
    vec3 powFriendlyObjectNormal = abs(vObjectNormal) + vec3(0.001);

    vec3 weights = pow(powFriendlyObjectNormal, vec3(uBlendingCoefficient));
    weights /= weights.x + weights.y + weights.z;
    vec3 baseColor = xyColor * weights.z + xzColor * weights.y + zyColor * weights.x;

    // normal mapping
    vec3 xyNormalMap = unpack8bitNormalmap(texture(uNormalXY, vObjectPos.xy / (uTexScaleXY * scale)).rgb);
    vec3 xzNormalMap1 = unpack8bitNormalmap(texture(uNormalXZ, vObjectPos.xz / (uTexScaleXZ * scale)).rgb);
    vec3 xzNormalMap2 = unpack8bitNormalmap(texture(uNormalXZDirt, vObjectPos.xz / (uTexScaleXZ * scale)).rgb);
    vec3 xzNormalMap = mix(xzNormalMap2, xzNormalMap1, min(1, vGrass * 2));
    vec3 zyNormalMap = unpack8bitNormalmap(texture(uNormalZY, vObjectPos.zy / (uTexScaleZY * scale)).rgb);

    vec3 xyNormal = xyNormalMap.xyz * sign(vObjectNormal.z);
    vec3 xzNormal = xzNormalMap.xzy * sign(vObjectNormal.y);
    vec3 zyNormal = zyNormalMap.zyx * sign(vObjectNormal.x);

    vec3 normalWeights = abs(vObjectNormal);
    normalWeights /= normalWeights.x + normalWeights.y + normalWeights.z;
    vec3 objNormal = xyNormal * normalWeights.z + xzNormal * normalWeights.y + zyNormal * normalWeights.x;
    color = baseColor;
    normal = objNormal;
}

void main()
{
    INIT_CHANNELS;

    float camDis = distance(vWorldPos, uCameraPosition);
    float lod = log(1 + camDis / uLodRefDist) / log(uLodFactor);
    float lodScale = pow(2,floor(lod));
    float lodFrac = fract(lod);

    vec3 baseColor = vec3(0);
    vec3 normal = vec3(0);
    // "normal" lod-aware terrain
    terrainFunction(lodScale, baseColor, normal);

    if ( lodFrac < uLodTransitionPart )
    {
        // blending region
        vec3 baseColor1;
        vec3 normal1;

        terrainFunction(lodScale / 2, baseColor1, normal1);

        float alpha = smoothstep(0, uLodTransitionPart, lodFrac);
        baseColor = mix(baseColor1, baseColor, alpha);
        normal    = mix(normal1,    normal,    alpha);
    }

    // color modulation
    baseColor *= vColor;

    // illumination
    normal = normalize(mat3(uModelMatrix) * normal);
    vec3 color = lighting(vWorldPos, normal, baseColor, uSpecularColor);

    float gmod = smoothstep(0.66, 0.5, vGrass);
    gmod = mix(gmod, 1, clamp(camDis / 100, 0.5, 1));
    color *= gmod;

    OUTPUT_Color(color);
    OUTPUT_Normal(vWorldNormal);
    OUTPUT_Position(vWorldPos);
}

